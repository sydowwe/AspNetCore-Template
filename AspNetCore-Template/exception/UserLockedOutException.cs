namespace AspNetCore_Template.exception;

public class UserLockedOutException(int lockOutTime) : Exception($"Locked out for {lockOutTime} minutes")
{
}