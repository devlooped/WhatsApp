namespace Devlooped.WhatsApp;

public class BsuidTests
{
    [Theory]
    // Valid: ISO 3166 alpha-2 prefix + alphanumeric suffix
    [InlineData("US.13491208655302741918", true)]   // docs example
    [InlineData("AR.aBc123XyZ", true)]              // mixed-case suffix
    [InlineData("gb.AlphaNum123", true)]            // lowercase country code
    [InlineData("US.A", true)]                      // single-char suffix
    // Invalid: phone numbers (all digits, no dot)
    [InlineData("5491122334455", false)]
    [InlineData("541122334455", false)]
    [InlineData("12025551234", false)]
    // Invalid: wrong prefix (digits instead of letters)
    [InlineData("54.aBc123XyZ", false)]
    [InlineData("1.ABCdef", false)]
    // Invalid: wrong prefix length
    [InlineData("USA.abc123", false)]               // 3-letter prefix
    [InlineData("U.abc123", false)]                 // 1-letter prefix
    // Invalid: structural issues
    [InlineData(".abc", false)]
    [InlineData("US.", false)]
    [InlineData("US.abc-def", false)]               // hyphen in suffix
    [InlineData("", false)]
    public void DetectsBusinessScopedUserId(string id, bool expected)
        => Assert.Equal(expected, User.IsBusinessScopedUserId(id));

    [Fact]
    public void PhoneOnlyUser_IsNotBSUID()
    {
        var user = new User("kzu", "5491122334455", "5491122334455");
        Assert.False(user.IsBSUID);
        Assert.Equal("541122334455", user.Number);
    }

    [Fact]
    public void ArgentinaPreMigration_NotMisclassifiedAsBSUID()
    {
        // Regression: Id is unnormalized ("549..."), Number becomes "54..." after normalization.
        // Pattern-based detection must not treat this as a BSUID.
        var user = new User("kzu", "5491122334455", "5491122334455");
        Assert.False(user.IsBSUID);
        Assert.NotEqual(user.Id, user.Number);
    }

    [Fact]
    public void PrivacyUser_HasNullNumber_IsBSUID()
    {
        var user = new User("kzu", "AR.aBc123XyZ", null);
        Assert.True(user.IsBSUID);
        Assert.Null(user.Number);
    }

    [Fact]
    public void MixedUser_HasBSUIDAndPhone()
    {
        var user = new User("kzu", "AR.aBc123XyZ", "5491122334455");
        Assert.True(user.IsBSUID);
        Assert.Equal("541122334455", user.Number);
    }

    [Fact]
    public void PhoneOnlyUser_NoNumberArg_NumberEqualsNormalizedId()
    {
        // When number is omitted and Id is a phone, Number is auto-set from Id.
        var user = new User("kzu", "5491122334455");
        Assert.False(user.IsBSUID);
        Assert.Equal("541122334455", user.Number);
    }

    [Fact]
    public void PhoneOnlyUser_ExplicitNullNumber_NumberEqualsNormalizedId()
    {
        // Explicit null behaves the same as omitting the number argument.
        var user = new User("kzu", "5491122334455", null);
        Assert.False(user.IsBSUID);
        Assert.Equal("541122334455", user.Number);
    }

    [Fact]
    public void PhoneOnlyUser_LeadingPlus_NumberStripsPlus()
    {
        var user = new User("kzu", "+12025551234");
        Assert.False(user.IsBSUID);
        Assert.Equal("12025551234", user.Number);
    }

    [Fact]
    public void BsuidUser_NoNumberArg_NumberRemainsNull()
    {
        // BSUID users with no phone number keep Number = null even when number is omitted.
        var user = new User("kzu", "AR.aBc123XyZ");
        Assert.True(user.IsBSUID);
        Assert.Null(user.Number);
    }

    [Fact]
    public void PhoneId_GoesInToField_NotRecipient()
    {
        Assert.Equal("5491122334455", WhatsAppClientExtensions.ToField("5491122334455"));
        Assert.Null(WhatsAppClientExtensions.RecipientField("5491122334455"));
    }

    [Fact]
    public void Bsuid_GoesInRecipientField_NotTo()
    {
        // Meta keeps recipient_type="individual" and puts the BSUID in the newer
        // "recipient" field — not recipient_type="business_scoped_user_id" / to.
        Assert.Null(WhatsAppClientExtensions.ToField("AR.aBc123XyZ"));
        Assert.Equal("AR.aBc123XyZ", WhatsAppClientExtensions.RecipientField("AR.aBc123XyZ"));
    }

    [Fact]
    public void PreferAddress_UsesPhoneWhenAvailable()
    {
        var mixed = new User("kzu", "AR.aBc123XyZ", "5491122334455");
        Assert.Equal("541122334455", WhatsAppClientExtensions.PreferAddress(mixed));

        var privacy = new User("kzu", "AR.aBc123XyZ", null);
        Assert.Equal("AR.aBc123XyZ", WhatsAppClientExtensions.PreferAddress(privacy));

        var phone = new User("kzu", "5491122334455");
        Assert.Equal("541122334455", WhatsAppClientExtensions.PreferAddress(phone));
    }
}
