using AspNetCore_Template.exception;
using AspNetCore_Template.helper;
using AspNetCore_Template.model.DTO;
using AspNetCore_Template.model.DTO.request.user;
using AspNetCore_Template.model.DTO.response.user;
using AspNetCore_Template.model.entity;
using AspNetCore_Template.security;
using AutoMapper;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Identity;
using QRCoder;

namespace AspNetCore_Template.service;

public interface IUserService
{
    Task<ServiceResult<TwoFactorAuthResponse>> RegisterUserAsync(RegistrationRequest registrationRequest);
    Task<ServiceResult<GoogleSignInResponse>> GoogleSignInAsync(GoogleSignInRequest googleSignInRequest);
    Task<ServiceResult<LoginResponse>> LoginUserAsync(PasswordLoginRequest loginRequest);
    Task<ServiceResult> ValidateTwoFactorAuthForLoginAsync(TwoFactorAuthLoginRequest request);
    Task Logout();
    Task<ServiceResult> ResendConfirmationEmail(long? userId);
    Task<ServiceResult> ConfirmEmail(long? userId, string? token);
    Task<ServiceResult> ForgottenPassword(string email);
    Task<ServiceResult> ResetPassword(ResetPasswordRequest request);
    Task<ServiceResult> ChangeEmailAsync(ChangeEmailRequest request);
    Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request);
    Task<ServiceResult<TwoFactorAuthResponse>> ToggleTwoFactorAuthAsync(VerifyUserRequest request);
    Task<ServiceResult> DeleteUserAccountAsync(VerifyUserRequest request);
    Task<ServiceResult> ChangeCurrentLocaleAsync(AvailableLocales locale);
    Task<bool> GetTwoFactorAuthStatusAsync();
    Task<UserResponse> GetLoggedUserDataAsync();
    Task<ServiceResult<string>> GenerateNewTwoFactorAuthQrCodeAsync(VerifyUserRequest request);
    Task<ServiceResult<IEnumerable<string>>> GenerateNewTwoFactorAuthRecoveryCodesAsync(VerifyUserRequest request);
}

public class UserService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILoggedUserService loggedUserService,
    IGoogleRecaptchaService googleRecaptchaService,
    IMyEmailSender<User> emailSender,
    IMapper mapper,
    IConfiguration configuration) : IUserService
{
    public async Task<ServiceResult> ChangeCurrentLocaleAsync(AvailableLocales locale)
    {
        var user = await GetLoggedUserAsync();
        user.CurrentLocale = locale;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? ServiceResult.Successful()
            : ServiceResult.Error(ServiceResultErrorType.IdentityError, result.Errors.ToString());
    }

    #region Authorization

    public async Task<ServiceResult<TwoFactorAuthResponse>> RegisterUserAsync(RegistrationRequest registration)
    {
        if (!await googleRecaptchaService.VerifyRecaptchaAsync(registration.RecaptchaToken, "register"))
            return ServiceResult<TwoFactorAuthResponse>.Error(
                ServiceResultErrorType.BadRequest,
                "Wrong captcha token  or action"
            );

        var newUser = new User
        {
            Email = registration.Email,
            UserName = registration.Email,
            TwoFactorEnabled = registration.TwoFactorEnabled,
            CurrentLocale = registration.CurrentLocale,
            Timezone = TimeZoneInfo.FindSystemTimeZoneById(registration.Timezone)
        };
        var result = await userManager.CreateAsync(newUser, registration.Password);
        if (!result.Succeeded)
            return result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail")
                ? ServiceResult<TwoFactorAuthResponse>.Error(ServiceResultErrorType.Conflict,
                    "User already exists with EMAIL: " + newUser.Email)
                : ServiceResult<TwoFactorAuthResponse>.Error(ServiceResultErrorType.BadRequest,
                    "Failed to register user because: " + string.Join(", ", result.Errors.Select(e => e.Description)));

        await SetDefaultSettingsAsync(newUser.Id);
        await SendConfirmationEmail(newUser);
        return await SetUpTwoFactorAuth(newUser);
    }

    private async Task<ServiceResult<string>> GetEmailFromGoogleSignInCode(string code)
    {
        var tokenResponse = await new AuthorizationCodeInstalledApp(
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = Helper.GetEnvVar("OAUTH2_GOOGLE_CLIENT_ID"),
                    ClientSecret = Helper.GetEnvVar("OAUTH2_GOOGLE_CLIENT_SECRET")
                }
            }),
            new LocalServerCodeReceiver()
        ).Flow.ExchangeCodeForTokenAsync("user", code, "<Your Redirect URI>", CancellationToken.None);
        var idToken = tokenResponse?.IdToken;
        if (idToken == null) return ServiceResult<string>.Error(ServiceResultErrorType.BadRequest, "Invalid Google login code");

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        var email = payload.Email;
        if (email == null) return ServiceResult<string>.Error(ServiceResultErrorType.BadRequest, "Email not found in token");
        return ServiceResult<string>.Successful(email);
    }

    public async Task<ServiceResult<GoogleSignInResponse>> GoogleSignInAsync(GoogleSignInRequest googleSignInRequest)
    {
        if (!await googleRecaptchaService.VerifyRecaptchaAsync(googleSignInRequest.RecaptchaToken, "login"))
            return ServiceResult<GoogleSignInResponse>.Error(ServiceResultErrorType.BadRequest,
                "Wrong captcha token or action");
        var emailResult = await GetEmailFromGoogleSignInCode(googleSignInRequest.Code);
        if (!emailResult.Succeeded) return ServiceResult<GoogleSignInResponse>.Error(emailResult);
        var email = emailResult.Data!.ToLower();
        var userResult = await GetByEmailAsync(email);
        if (userResult.Succeeded)
        {
            var user = userResult.Data!;
            await signInManager.SignInAsync(user, googleSignInRequest.StayLoggedIn, "Google");
            user.Timezone = TimeZoneInfo.FindSystemTimeZoneById(googleSignInRequest.Timezone);
            await userManager.UpdateAsync(user);
            return ServiceResult<GoogleSignInResponse>.Successful(new GoogleSignInResponse
            {
                Email = email,
                CurrentLocale = user.CurrentLocale
            });
        }

        return ServiceResult<GoogleSignInResponse>.Successful(new GoogleSignInResponse());
    }

    public async Task<ServiceResult<LoginResponse>> LoginUserAsync(PasswordLoginRequest loginRequest)
    {
        if (!await googleRecaptchaService.VerifyRecaptchaAsync(loginRequest.RecaptchaToken, "login"))
            return ServiceResult<LoginResponse>.Error(ServiceResultErrorType.BadRequest,
                "Wrong captcha token or action");

        var userResult = await GetByEmailAsync(loginRequest.Email.ToLower());
        if (!userResult.Succeeded) return ServiceResult<LoginResponse>.Error(userResult.ErrorType, userResult.ErrorMessage);

        var user = userResult.Data!;
        var result = await signInManager.PasswordSignInAsync(user, loginRequest.Password,
            loginRequest.StayLoggedIn, true);
        if (result.IsLockedOut)
        {
            var lockoutDuration = user.LockoutEnd!.Value - DateTimeOffset.Now;
            var minutes = (int)lockoutDuration.TotalMinutes;
            var seconds = lockoutDuration.Seconds;
            return ServiceResult<LoginResponse>.Error(ServiceResultErrorType.UserLockedOut,
                $"User locked out for {minutes}m {seconds}s");
        }

        if (result.IsNotAllowed)
        {
            if (!user.EmailConfirmed)
                return ServiceResult<LoginResponse>.Error(ServiceResultErrorType.EmailNotConfirmed,
                    "Confirm your email before logging in");
            await userManager.AccessFailedAsync(user);
            return ServiceResult<LoginResponse>.Error(ServiceResultErrorType.AuthenticationFailed,
                "Wrong email or password");
        }

        if (result is { Succeeded: false, RequiresTwoFactor: false })
            return ServiceResult<LoginResponse>.Error(ServiceResultErrorType.InternalServerError, result.ToString());
        user.Timezone = TimeZoneInfo.FindSystemTimeZoneById(loginRequest.Timezone);
        await userManager.UpdateAsync(user);
        return ServiceResult<LoginResponse>.Successful(
            new LoginResponse
            {
                Email = user.Email!,
                RequiresTwoFactor = result.RequiresTwoFactor,
                CurrentLocale = user.CurrentLocale
            });
    }

    public async Task<ServiceResult> ValidateTwoFactorAuthForLoginAsync(TwoFactorAuthLoginRequest request)
    {
        var result =
            await signInManager.TwoFactorAuthenticatorSignInAsync(request.Token, request.StayLoggedIn,
                false);
        if (!result.Succeeded) ServiceResult.Error(ServiceResultErrorType.InternalServerError, result.ToString());

        return ServiceResult.Successful();
    }

    public async Task Logout()
    {
        await signInManager.SignOutAsync();
    }

    #endregion

    //TODO email sender and test these methods

    #region emailSenderNeeded

    public async Task<ServiceResult> ForgottenPassword(string email)
    {
        var userResult = await GetByEmailAsync(email);
        if (!userResult.Succeeded) return ServiceResult.Error(userResult.ErrorType, userResult.ErrorMessage);

        var user = userResult.Data!;
        if (!await userManager.IsEmailConfirmedAsync(user))
            return ServiceResult.Error(ServiceResultErrorType.EmailNotConfirmed, "User doesn't have email confirmed");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = new Uri($"{Helper.GetEnvVar("PAGE_URL")}/reset-password?userId={user.Id}&token={token}");
        await emailSender.SendPasswordResetLinkAsync(user, email, resetLink.ToString());
        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ResetPassword(ResetPasswordRequest request)
    {
        var userResult = await GetByIdAsync(request.UserId);
        if (!userResult.Succeeded) return ServiceResult.Error(userResult.ErrorType, userResult.ErrorMessage);

        var result = await userManager.ResetPasswordAsync(userResult.Data!, request.Token, request.NewPassword);
        return result.Succeeded
            ? ServiceResult.Successful()
            : ServiceResult.Error(ServiceResultErrorType.IdentityError, result.Errors.ToString());
    }

    private async Task SendConfirmationEmail(User user)
    {
        if (!user.EmailConfirmed)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink =
                new Uri($"{Helper.GetEnvVar("PAGE_URL")}/confirm-email?userId={user.Id}&token={token}");
            await emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmationLink.ToString());
        }
    }

    public async Task<ServiceResult> ResendConfirmationEmail(long? userId)
    {
        if (!userId.HasValue) return ServiceResult.Error(ServiceResultErrorType.BadRequest, "UserId must be supplied");

        var userResult = await GetByIdAsync(userId.Value);
        if (!userResult.Succeeded) return ServiceResult.Error(userResult.ErrorType, userResult.ErrorMessage);

        await SendConfirmationEmail(userResult.Data!);
        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ConfirmEmail(long? userId, string? token)
    {
        if (!userId.HasValue || string.IsNullOrEmpty(token))
            return ServiceResult.Error(ServiceResultErrorType.BadRequest, "UserId and token must be supplied");

        var userResult = await GetByIdAsync(userId.Value);
        if (!userResult.Succeeded) return ServiceResult.Error(userResult.ErrorType, userResult.ErrorMessage);

        var result = await userManager.ConfirmEmailAsync(userResult.Data!, token);
        return result.Succeeded
            ? ServiceResult.Successful()
            : ServiceResult.Error(ServiceResultErrorType.IdentityError, result.Errors.ToString());
    }

    #endregion


    #region UserSettings

    public async Task<bool> GetTwoFactorAuthStatusAsync()
    {
        return loggedUserService.GetLoggedUserTwoFactorAuthStatus() ?? (await GetLoggedUserAsync()).TwoFactorEnabled;
    }

    public async Task<UserResponse> GetLoggedUserDataAsync()
    {
        var loggedUser = await GetLoggedUserAsync();
        return mapper.Map<UserResponse>(loggedUser);
    }

    public async Task<ServiceResult> ChangeEmailAsync(ChangeEmailRequest request)
    {
        var user = await GetLoggedUserAsync();
        var verifyResult = await VerifyUserAsync(request, user);
        if (!verifyResult.Succeeded) return verifyResult;

        //TODO Bez 2fa treba poslat email s tokenom a ten overit spravi takisto aj pri hesle
        // IdentityResult? result;
        // if (user.TwoFactorEnabled)
        // {
        //     result = await userManager.ChangeEmailAsync(user, request.NewEmail, await userManager.GenerateChangeEmailTokenAsync(user,request.NewEmail));
        // }
        // else
        // {
        //     result = await userManager.ChangeEmailAsync(user, request.NewEmail, request.TwoFactorAuthToken);
        // }
        var result = await userManager.ChangeEmailAsync(user, request.NewEmail,
            await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail));
        if (!result.Succeeded) return ServiceResult.Error(ServiceResultErrorType.IdentityError, result.Errors.ToString());

        await userManager.UpdateSecurityStampAsync(user);
        await signInManager.SignOutAsync();
        return ServiceResult.Successful();
    }

    public async Task<ServiceResult<TwoFactorAuthResponse>> ToggleTwoFactorAuthAsync(VerifyUserRequest request)
    {
        var user = await GetLoggedUserAsync();
        var verifyResult = await VerifyUserAsync(request, user);
        if (!verifyResult.Succeeded) return (ServiceResult<TwoFactorAuthResponse>)verifyResult;

        user.TwoFactorEnabled = !user.TwoFactorEnabled;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return ServiceResult<TwoFactorAuthResponse>.Error(ServiceResultErrorType.IdentityError,
                result.Errors.ToString());

        return await SetUpTwoFactorAuth(user);
    }

    public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await GetLoggedUserAsync();
        var twoFactorAuthResult = await ValidateTwoFactorAuthAsync(user, request.TwoFactorAuthToken);
        if (!twoFactorAuthResult.Succeeded) return twoFactorAuthResult;

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) return ServiceResult.Error(ServiceResultErrorType.IdentityError, result.Errors.ToString());

        await userManager.UpdateSecurityStampAsync(user);
        await signInManager.SignOutAsync();
        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteUserAccountAsync(VerifyUserRequest request)
    {
        var user = await GetLoggedUserAsync();
        var verifyResult = await VerifyUserAsync(request, user);
        if (!verifyResult.Succeeded) return verifyResult;

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded
            ? ServiceResult.Successful()
            : ServiceResult.Error(ServiceResultErrorType.IdentityError, result.Errors.ToString());
    }

    public async Task<ServiceResult<string>> GenerateNewTwoFactorAuthQrCodeAsync(VerifyUserRequest request)
    {
        var user = await GetLoggedUserAsync();
        var verifyResult = await VerifyUserAsync(request, user);
        if (!verifyResult.Succeeded) return (ServiceResult<string>)verifyResult;

        var qrCodeResult = await GenerateNewTwoFactorAuthQrCodeAsync(user);
        if (!qrCodeResult.Succeeded) return ServiceResult<string>.Error(qrCodeResult.ErrorType, qrCodeResult.ErrorMessage);

        return ServiceResult<string>.Successful(qrCodeResult.Data!);
    }

    public async Task<ServiceResult<IEnumerable<string>>> GenerateNewTwoFactorAuthRecoveryCodesAsync(
        VerifyUserRequest request)
    {
        var user = await GetLoggedUserAsync();
        var verifyResult = await VerifyUserAsync(request, user);
        if (!verifyResult.Succeeded) return (ServiceResult<IEnumerable<string>>)verifyResult;

        var recoveryCodesResult = await GenerateNewTwoFactorAuthRecoveryCodesAsync(user);
        if (!recoveryCodesResult.Succeeded)
            return ServiceResult<IEnumerable<string>>.Error(recoveryCodesResult.ErrorType,
                recoveryCodesResult.ErrorMessage);

        return ServiceResult<IEnumerable<string>>.Successful(recoveryCodesResult.Data!);
    }

    private async Task<ServiceResult> ValidateTwoFactorAuthAsync(User user, string? token)
    {
        if (!user.TwoFactorEnabled) return ServiceResult.Successful();
        if (string.IsNullOrEmpty(token))
            return ServiceResult.Error(ServiceResultErrorType.TwoFactorAuthRequired,
                "Two-factor authentication is required to proceed.");

        var isTokenValid = await userManager.VerifyTwoFactorTokenAsync(user,
            TokenOptions.DefaultAuthenticatorProvider, token);
        if (!isTokenValid)
            return ServiceResult.Error(ServiceResultErrorType.InvalidTwoFactorAuthToken,
                "Invalid two-factor authentication token.");

        return ServiceResult.Successful();
    }

    private async Task<ServiceResult> VerifyUserAsync(VerifyUserRequest request, User user)
    {
        var twoFactorAuthResult = await ValidateTwoFactorAuthAsync(user, request.TwoFactorAuthToken);
        if (!twoFactorAuthResult.Succeeded) return twoFactorAuthResult;

        var result = await userManager.CheckPasswordAsync(user, request.Password);
        return result
            ? ServiceResult.Successful()
            : ServiceResult.Error(ServiceResultErrorType.AuthenticationFailed, "Wrong password");
    }

    #endregion

    #region Private Methods

    #region MyRegion

    private async Task<ServiceResult<TwoFactorAuthResponse>> SetUpTwoFactorAuth(User user)
    {
        if (!user.TwoFactorEnabled)
            return ServiceResult<TwoFactorAuthResponse>.Successful(
                new TwoFactorAuthResponse
                {
                    TwoFactorEnabled = false
                });

        var qrCodeResult = await GenerateNewTwoFactorAuthQrCodeAsync(user);
        if (!qrCodeResult.Succeeded) return ServiceResult<TwoFactorAuthResponse>.Error(qrCodeResult.ErrorType, qrCodeResult.ErrorMessage);

        var recoveryCodesResult = await GenerateNewTwoFactorAuthRecoveryCodesAsync(user);
        if (!recoveryCodesResult.Succeeded)
            return ServiceResult<TwoFactorAuthResponse>.Error(recoveryCodesResult.ErrorType,
                recoveryCodesResult.ErrorMessage);

        return ServiceResult<TwoFactorAuthResponse>.Successful(
            new TwoFactorAuthResponse
            {
                TwoFactorEnabled = user.TwoFactorEnabled,
                QrCode = qrCodeResult.Data,
                RecoveryCodes = recoveryCodesResult.Data
            });
    }

    private async Task<ServiceResult<string>> GenerateNewTwoFactorAuthQrCodeAsync(User user)
    {
        var result = await userManager.ResetAuthenticatorKeyAsync(user);
        if (!result.Succeeded) return ServiceResult<string>.Error(ServiceResultErrorType.IdentityError, result.Errors.ToString());

        var totpAuthenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
        return string.IsNullOrEmpty(totpAuthenticatorKey)
            ? ServiceResult<string>.Error(ServiceResultErrorType.NotFound, "totpAuthenticatorKey not found")
            : ServiceResult<string>.Successful(GenerateQrCode(totpAuthenticatorKey, user.Email!));
    }

    private string GenerateQrCode(string secretKey, string userEmail)
    {
        var appName = configuration.GetValue<string>("Application:Name") ??
                      throw new ArgumentNullException(nameof(configuration));
        var otpAuthUrl = $"otpauth://totp/{appName}:{userEmail}?secret={secretKey}&issuer={appName}&digits=6";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpAuthUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return Convert.ToBase64String(qrCode.GetGraphic(3));
    }

    private async Task<ServiceResult<IEnumerable<string>>> GenerateNewTwoFactorAuthRecoveryCodesAsync(User user)
    {
        var recoveryCodes = (await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5))?.ToList();
        return recoveryCodes is { Count: > 0 }
            ? ServiceResult<IEnumerable<string>>.Successful(recoveryCodes)
            : ServiceResult<IEnumerable<string>>.Error(ServiceResultErrorType.NotFound, "recoveryCodes not found");
    }

    #endregion

    private async Task SetDefaultSettingsAsync(long userId)
    {
        //TODO doplnit
    }

    private async Task<User> GetLoggedUserAsync()
    {
        var principal = loggedUserService.GetLoggedUserPrincipal();
        var user = await userManager.GetUserAsync(principal);
        if (user == null) throw new UserByPrincipalNotFoundException(principal);

        return user;
    }

    private async Task<ServiceResult<User>> GetByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user == null
            ? ServiceResult<User>.Error(ServiceResultErrorType.NotFound, $"User with EMAIL: '{email}' was not found")
            : ServiceResult<User>.Successful(user);
    }

    private async Task<ServiceResult<User>> GetByIdAsync(long id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        return user == null
            ? ServiceResult<User>.Error(ServiceResultErrorType.NotFound, $"User with ID: '{id}' was not found")
            : ServiceResult<User>.Successful(user);
    }

    #endregion
}