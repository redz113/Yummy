using System.ComponentModel.DataAnnotations;

namespace App.Validations
{
    public class CustomFileValidation : ValidationAttribute
    {
        private readonly int _maxFileSize;
        private readonly List<string> allowedExtensions = new List<string>() { ".jpeg", ".jpg", ".png"};
        public CustomFileValidation(int maxFileSize) {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file) {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return new ValidationResult($"Định dạng ảnh không hợp lệ.");
                }

                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult("Kích thước file ảnh quá lớn.");
                } 
            }
            return ValidationResult.Success;
        }
    }
}
