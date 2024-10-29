namespace AspNetCore_Template.exception;

public class ReCaptchaException(string? message) : ApplicationException(message);