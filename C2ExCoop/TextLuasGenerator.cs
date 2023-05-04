﻿using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace RM2ExCoop.C2ExCoop
{
    internal class TextLuasGenerator
    {
        readonly string _dialogsPath;
        readonly string _coursesPath;

        public TextLuasGenerator(string dialogsPath, string coursesPath)
        {
            _dialogsPath = dialogsPath;
            _coursesPath = coursesPath;
        }

        public void Generate(string outputDir)
        {
            // Dialog.h
            if (!File.Exists(_dialogsPath))
            {
                Logger.Warn("There were no dialogs.h file generated. Skipping it.");
            }
            else
            {
                new FileObject(_dialogsPath).
                    Replace(new Regex("DEFINE_DIALOG"), "smlua_text_utils_dialog_replace").
                    Replace(new Regex("_\\("), "(").
                    Replace(new Regex("\\\\n\\\\"), "\\")
                    .ApplyAndSave(Path.Join(outputDir, "dialogs.lua"));
            }

            // Courses.h
            if (!File.Exists(_coursesPath))
            {
                Logger.Warn("There were no courses.h file generated. Skipping it.");
            }
            else
            {
                new FileObject(_coursesPath).
                    Replace(new Regex("_\\("), "(").
                    Replace(new Regex("COURSE_ACTS"), "smlua_text_utils_course_acts_replace").
                    Replace(new Regex("CASTLE_SECRET_STARS"), "smlua_text_utils_castle_secret_stars_replace").
                    Replace(new Regex("SECRET_STAR\\((\\d+),"), "smlua_text_utils_secret_star_replace($1 + 1,").
                    Replace(new Regex("EXTRA_TEXT"), "smlua_text_utils_extra_text_replace")
                    .ApplyAndSave(Path.Join(outputDir, "courses.lua"));
            }
        }
    }
}
