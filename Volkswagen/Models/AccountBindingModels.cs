using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;



namespace Volkswagen.Models
{
    // 用作 AccountController 操作的参数的模型。

    public class AddExternalLoginBindingModel
    {
        [Required]
        [Display(Name = "外部访问令牌")]
        public string ExternalAccessToken { get; set; }
    }

    public class ChangePasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "当前密码")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterBindingModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

//        [Required]
//        [Display(Name = "First Name")]
//        public string FirstName { get; set; }

//        [Required]
//        [Display(Name = "Last Name")]
//        public string LastName { get; set; }

//        [Required]
//        [Display(Name = "电子邮件信箱")]
//        public string Email { get; set; }

        public ApplicationUser GetUser()
        {
            var user = new ApplicationUser()
            {
                UserName = this.UserName,
//                FirstName = this.FirstName,
//                LastName = this.LastName,
//                Email = this.Email,
            };
            return user;
        }

    }





    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
    }

    public class RemoveLoginBindingModel
    {
        [Required]
        [Display(Name = "登录提供程序")]
        public string LoginProvider { get; set; }

        [Required]
        [Display(Name = "提供程序密钥")]
        public string ProviderKey { get; set; }
    }

    public class SetPasswordBindingModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }


    public class EditUserViewModel
    {
        public EditUserViewModel() { }
        // Allow Initialization with an instance of ApplicationUser:
        public EditUserViewModel(ApplicationUser user)
        {
            this.UserName = user.UserName;
 //           this.FirstName = user.FirstName;
//            this.LastName = user.LastName;
//            this.Email = user.Email;
        }
        
        [Required]
        [Display(Name = "使用者账号")]
        public string UserName { get; set; }

//        [Required]
//        [Display(Name = "名")]
//        public string FirstName { get; set; }

//        [Required]
//        [Display(Name = "姓")]
//       public string LastName { get; set; }

//        [Required]
//        [Display(Name = "电子邮件信箱")]
//        public string Email { get; set; }
        
    }

    public class SelectUserRolesViewModel
    {
        public SelectUserRolesViewModel()       
        {
            this.Roles = new List<SelectRoleEditorViewModel>();         
        }
        
        // Enable initialization with an instance of ApplicationUser:
        public SelectUserRolesViewModel(ApplicationUser user)
            : this()
        {
            this.UserName = user.UserName;
//            this.FirstName = user.FirstName;
//            this.LastName = user.LastName;
            var Db = new ApplicationDbContext();
            // Add all available roles to the list of EditorViewModels:
            var allRoles = Db.Roles;
            foreach (var role in allRoles)
            {
                // An EditorViewModel will be used by Editor Template:
                var rvm = new SelectRoleEditorViewModel(role);
                this.Roles.Add(rvm);
                
            }
            // Set the Selected property to true for those roles for
            // which the current user is a member:
            foreach (var userRole in user.Roles)
                {
                var checkUserRole =
                    this.Roles.Find(r => r.RoleName == userRole.Role.Name);
                checkUserRole.Selected = true;
                
            }
            
        }
        public string UserName { get; set; }
//        public string FirstName { get; set; }
//        public string LastName { get; set; }
        public List<SelectRoleEditorViewModel> Roles { get; set; }
        
    }
    
    // Used to display a single role with a checkbox, within a list structure:
    public class SelectRoleEditorViewModel
    {
        public SelectRoleEditorViewModel() { }
        public SelectRoleEditorViewModel(IdentityRole role)
        {
            this.RoleName = role.Name;
        }
        public bool Selected { get; set; }

        [Required]
        public string RoleName { get; set; }
        
    }
    
}


