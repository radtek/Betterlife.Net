﻿using System;

namespace Database.Domain.Enums
{
    /// <summary>
    /// 在线编辑器的类型
    /// </summary>
    public enum EnumOnlineEditor
    {
        /// <summary>
        /// CKEditor
        /// </summary>
        /// <see cref="http://ckeditor.com/"/>
        CKEDITOR = 1,
        /// <summary>
        /// UEDITOR
        /// </summary>
        /// <see cref="http://ueditor.baidu.com/"/>
        UEDITOR = 4
    }

    /// <summary>
    /// 系统管理员扮演角色
    /// </summary>
    public class EnumRoletype
    {
        /// <summary>
        /// 超级管理员
        /// </summary>
        public const char Superadmin = '0';
        /// <summary>
        /// 普通管理员
        /// </summary>
        public const char Manager = '1';

        /// <summary>
        /// 显示系统管理员扮演角色
        /// </summary>
        public static String RoletypeShow(char Type)
        {
            switch (Type)
            {
                case Superadmin:
                    return "超级管理员";
                case Manager:
                    return "普通管理员";
            }
            return "未知";
        }

        /// <summary>
        /// 根据系统管理员扮演角色显示文字获取系统管理员扮演角色
        /// </summary>
        public static string RoletypeByShow(string Content)
        {
            char result=char.MinValue;
            switch (Content)
            {
                case "超级管理员":
                    result= Superadmin;
                    break;
                case "普通管理员":
                    result= Manager;
                    break;
            }
            
            return result.ToString();
        }
    }

    /// <summary>
    /// 视野
    /// </summary>
    public class EnumSeescope
    {
        /// <summary>
        /// 只能查看自己的信息
        /// </summary>
        public const char Self = '0';
        /// <summary>
        /// 查看所有的信息
        /// </summary>
        public const char All = '1';

        /// <summary>
        /// 显示视野
        /// </summary>
        public static String SeescopeShow(char Type)
        {
            switch (Type)
            {
                case Self:
                    return "只能查看自己的信息";
                case All:
                    return "查看所有的信息";
            }
            return "未知";
        }

        /// <summary>
        /// 根据视野显示文字获取视野
        /// </summary>
        public static string SeescopeByShow(string Content)
        {
            char result = char.MinValue;
            switch (Content)
            {
                case "只能查看自己的信息":
                    result= Self;
                    break;
                case "查看所有的信息":
                    result= All;
                    break;
            }
            return result.ToString();
        }
    }
}