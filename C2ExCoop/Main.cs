﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RM2ExCoop.C2ExCoop
{
    internal class Main
    {
        public static void Run(MainWindow window, string modName, string modDesc, bool commentSOM, bool removeFlags, bool removePaintings, bool removeTrajectories, bool tryFixFog, bool dontUseCameraSpecific, string entryLevel)
        {
            string rootDir = Directory.GetCurrentDirectory();
            string modDir = Path.Join(rootDir, "mod");

            if (Directory.Exists(modDir))
            {
                window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Deleting old mod directory")));
                Directory.Delete(modDir, true);
            }

            {
                string outputDir = Path.Join(rootDir, "output");
                window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Copying all C files into new mod directory")));
                RM2C.Utils.CopyDirectory(outputDir, modDir);
            }

            DirectoryInfo levelsDir = new(Path.Join(modDir, "levels"));
            List<(string, string)> toRename = new();
            List<string> toDelete = new();
            List<(string, string)> movTexs = new();

            (string, string)[] filesToCheck = new (string, string)[]
            {
                ("custom.script.c", "script.c"),
                ("custom.geo.c", "geo.c"),
                ("custom.leveldata.c", "leveldata.c"),
                ("textureNew.inc.c", "texture.inc.c")
            };

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Processing all level files")));

            foreach (DirectoryInfo lvl in levelsDir.GetDirectories())
            {
                window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Processing level " + lvl.Name)));

                FileInfo[] files = lvl.GetFiles();

                for (int i = 0; i < files.Length; ++i)
                {
                    FileInfo file = files[i];

                    foreach (var (src, dest) in filesToCheck)
                    {
                        if (file.Name == src)
                        {
                            string destPath = file.FullName.Replace(src, dest);
                            toRename.Add((file.FullName, destPath));
                            toDelete.Add(destPath);

                            if (file.Name == "custom.script.c")
                                new FileObject(file.FullName).Replace(new Regex("custom_entry"), "entry").ApplyAndSave();
                        }
                    }

                    if (removePaintings && file.Name == "painting.inc.c")
                        toDelete.Add(file.FullName);
                    if (removeTrajectories && file.Name == "trajectory.inc.c")
                        toDelete.Add(file.FullName);
                }

                foreach (var dirs in lvl.GetDirectories())
                {
                    if (dirs.Name != "areas") continue;

                    foreach (var area in dirs.GetDirectories())
                    {
                    window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Processing area " + area.Name + " of " + lvl.Name)));

                        int movtexFiles = 0;

                        foreach (var areaFile in area.GetFiles())
                        {
                            if (areaFile.Name == "custom.model.inc.c")
                            {
                                FileObject file = new(areaFile.FullName);

                                if (commentSOM)
                                    file.Replace(new Regex("gsSPSetOtherMode"), "//gsSPSetOtherMode");
                                if (tryFixFog && Globals.AreasWithFog.Contains($"{lvl.Name}_{area.Name}_"))
                                {
                                    file.Replace(new Regex("gsDPSetCombineLERP"), "gsDPSetCombineMode").
                                        Replace(new Regex("0, 0, 0, SHADE, 0, 0, 0, SHADE"), "G_CC_SHADE").
                                        Replace(new Regex("0, 0, 0, COMBINED, 0, 0, 0, COMBINED"), "G_CC_PASS2").
                                        Replace(new Regex("TEXEL0, 0, SHADE, 0, 0, 0, 0, TEXEL0"), "G_CC_MODULATEIDECALA").
                                        Replace(new Regex("SHADE, 0, ENVIRONMENT, 0, 0, 0, 0, 1"), "G_CC_FADE").
                                        Replace(new Regex("TEXEL0, 0, SHADE, 0, 0, 0, 0, 1"), "G_CC_MODULATEI").
                                        Replace(new Regex("TEXEL0, 0, SHADE, 0, TEXEL0, 0, ENVIRONMENT, 0"), "G_CC_MODULATEIFADEA");
                                }

                                file.ApplyAndSave();
                            }
                            if (removePaintings && areaFile.Name == "painting.inc.c")
                                areaFile.Delete();
                            if (removeTrajectories && areaFile.Name == "trajectory.inc.c")
                                toDelete.Add(areaFile.FullName);
                            if (areaFile.Name == "movtext.inc.c" || areaFile.Name == "movtextNew.inc.c")
                                ++movtexFiles;
                        }

                        if (movtexFiles >= 2)
                            movTexs.Add((lvl.Name, area.Name));

                        foreach (var areaDir in area.GetDirectories())
                        {
                            if (removeFlags && lvl.Name == "castle_grounds" && area.Name == "1" && areaDir.Name == "11")
                                areaDir.Delete(true);
                        }
                    }
                }
            }

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Deleting useless level files")));
            foreach (string file in toDelete)
                File.Delete(file);

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Renaming some level files")));
            foreach (var (src, dest) in toRename)
                File.Move(src, dest);

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Renaming and moving .m64 sequences files")));
            string soundDir = Path.Join(modDir, "sound");
            DirectoryInfo seqDir = new(Path.Join(soundDir, "sequences", "us"));
            foreach (var file in seqDir.GetFiles())
            {
                string newFileName = Regex.Replace(file.Name, "Seq_.*_custom", "Seq_custom");
                file.MoveTo(Path.Join(soundDir, newFileName));
            }
            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Deleting sequences folder")));
            Directory.Delete(Path.Join(soundDir, "sequences"), true);

            string srcPath = Path.Join(modDir, "src");

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Generating main.lua file")));
            string seqPath = Path.Join(soundDir, "sequences.json");
            string starPosPath = Path.Join(srcPath, "game", "Star_Pos.inc.c");
            new MainLuaGenerator(modName, modDesc, seqPath, starPosPath, movTexs, dontUseCameraSpecific, entryLevel).Generate(modDir);

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Generating dialogs.lua and courses.lua files")));
            string dialogsPath = Path.Join(modDir, "text", "us", "dialogs.h");
            string coursesPath = Path.Join(modDir, "text", "us", "courses.h");
            new TextLuasGenerator(dialogsPath, coursesPath).Generate(modDir);

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Generating tweaks.lua file")));
            string tweaksPath = Path.Join(srcPath, "game", "tweaks.inc.c");
            new TweaksLuaGenerator(tweaksPath).Generate(modDir);

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Generating moving_textures.lua file")));
            string movingTexturePath = Path.Join(srcPath, "game", "moving_texture.inc.c");
            string scrollTargetsPath = Path.Join(srcPath, "game", "ScrollTargets.inc.c");
            new MovingTexturesLuaGenerator(movingTexturePath, scrollTargetsPath).Generate(modDir);

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("Deleting src folder")));
            Directory.Delete(srcPath, true);

            window.Dispatcher.BeginInvoke(() => window.Dispatcher.BeginInvoke(() => window.Log("C2ExCoop done.")));
        }
    }
}
