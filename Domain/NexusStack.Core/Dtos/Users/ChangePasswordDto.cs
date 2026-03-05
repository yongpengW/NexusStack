using System.ComponentModel.DataAnnotations;

namespace NexusStack.Core.Dtos.Users
{
    /// <summary>
    /// 修改密码 DTO
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// 旧密码
        /// </summary>
        [Required(ErrorMessage = "旧密码不能为空")]
        public string OldPassword { get; set; } = string.Empty;

        /// <summary>
        /// 新密码
        /// </summary>
        [Required(ErrorMessage = "新密码不能为空")]
        [MinLength(6, ErrorMessage = "新密码长度不能少于6位")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// 确认新密码
        /// </summary>
        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare(nameof(NewPassword), ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
