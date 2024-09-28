using System.ComponentModel.DataAnnotations;

namespace App.Validations
{
    public class CustomBirthdayValidation : ValidationAttribute
    {
        public CustomBirthdayValidation() { 
            
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            DateTime now = DateTime.Now;
            DateTime min = new DateTime(year: now.Year - 100, month: now.Month, day: now.Day);

            string validateString = $"Ngày sinh không hợp lệ.";
            
            if (value is DateTime date)
            {
                if (date > now || date < min)
                {
                    return new ValidationResult(validateString);
                }
            }
            return ValidationResult.Success;
        }
    }
}
