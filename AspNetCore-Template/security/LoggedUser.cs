using AspNetCore_Template.model.entity;

namespace AspNetCore_Template.security;

public class LoggedUser
{
    public long UserId { get; set; }
    public string Email { get; set; }
    public string Timezone { get; set; }
    public AvailableLocales Locale { get; set; }
    public bool? TwoFactorEnabled { get; set; }
    public IEnumerable<string> Roles { get; set; }
}