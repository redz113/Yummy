using AppFoods.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.UserViewModels
{
  public class AddUserClaimModel
  {     
        public List<bool> Claims { get; set; }
  }
}