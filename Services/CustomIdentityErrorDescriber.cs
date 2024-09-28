using Microsoft.AspNetCore.Identity;

namespace AppFoods.Services
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        //USER NAME

        public override IdentityError InvalidEmail(string? email)
        {
            return new IdentityError
            {
                Code = "InvalidEmail",
                Description = "Email không hợp lệ."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = "DuplicateEmail",
                Description = $"Email '{email}' đã được đăng ký với một tài khoản có trên hệ thống."
            };
        }


        //PASSWORD
        public override IdentityError PasswordMismatch()
        {
            return new IdentityError
            {
                Code = "PasswordMismatch",
                Description = "Mật khẩu không khớp."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = "PasswordRequiresDigit",
                Description = "Mật khẩu yêu cầu phải bao gồm chữ số."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = "PasswordRequiresLower",
                Description = "Mật khẩu yêu cầu phải bao gồm chữ thường."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = "PasswordRequiresUpper",
                Description = "Mật khẩu yêu cầu phải bao gồm chữ hoa."
            };
        }



    }
}
