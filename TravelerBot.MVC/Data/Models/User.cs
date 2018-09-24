using System;

namespace TravelerBot.MVC.Data.Models
{
    public class User
    {
        public Guid UserId { get; set; }

        public int AccountId { get; set; }

        public DateTime CreateAt { get; set; }

        public Guid RoleId { get; set; }

        public virtual Role Role { get; set; }

        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// Заблокирован ли пользователь.
        /// </summary>
        public bool IsBlocked { get; set; }

        public string AdditionalInfo { get; set; }
    }
}