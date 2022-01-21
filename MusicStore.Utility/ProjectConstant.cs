﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Utility
{
    public static class ProjectConstant
    {
        /* Değişkenin değerini program boyunca sabit olarak tutulmasını sağlar.
         * Const: (sabit) değer.*/

        public const string ResultNotFound = "Data Not  Found";

        //****************************************************//

        public const string Proc_CoverType_GetAll = "usp_GetCoverTypes";
        public const string Proc_CoverType_Get = "usp_GetCoverType";
        public const string Proc_CoverType_Update = "usp_UpdateCoverType";
        public const string Proc_CoverType_Delete = "usp_DeleteCoverType";
        public const string Proc_CoverType_Create = "usp_CreateCoverType";

        /*
        usp_GetCoverTypes
        usp_GetCoverType
        usp_UpdateCoverType
        usp_DeleteCoverType
        usp_CreateCoverType
        */

        public const string Role_User_Indi = "Individual Customer"; // Bireysel Müşteri
        public const string Role_User_Comp = "Company Customer";

        public const string Role_Admin = "Admin";

        public const string Role_User_Employee = "Employee";

    }
}
