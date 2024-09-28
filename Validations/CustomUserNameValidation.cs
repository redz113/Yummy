using System.ComponentModel.DataAnnotations;

namespace AppFoods.Validations
{
    public class CustomUserNameValidation : ValidationAttribute
    {
        public CustomUserNameValidation() { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string v = value as string;

            if (v.Contains('.'))
            {
                return new ValidationResult("Tên tài khoản không được chứa ký tự \'.\'");
            }
            return ValidationResult.Success;
        }
    }
}
