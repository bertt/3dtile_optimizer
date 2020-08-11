using B3dm.Tile;
using System;
using System.Diagnostics;
using System.IO;

namespace _3dtile_optimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var input_dir = @"D:\dev\github.com\bertt\mapbox_3dtiles_samples\samples\street";
            var output_dir = @"D:\dev\github.com\bertt\mapbox_3dtiles_samples\samples\street\output";

            var b3dms = Directory.GetFiles(input_dir, "*.b3dm");

            foreach(var b3dmfile in b3dms)
            {
                Console.Write(".");
                // read b3dm
                var f = File.OpenRead(b3dmfile);
                var b3dm = B3dmReader.ReadB3dm(f);
                var stream = new MemoryStream(b3dm.GlbData);

                // write glb
                var glbname = output_dir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(b3dmfile) + ".glb";
                File.WriteAllBytes(glbname, stream.ToArray());

                // run gltfpack to compress
                var startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = output_dir;
                startInfo.FileName = @"gltfpack.cmd";
                startInfo.Arguments = $"-i {Path.GetFileName(glbname)} -o {Path.GetFileName(glbname)}";
                Process.Start(startInfo);

                // must wait till gltpfpack is surely finished
                System.Threading.Thread.Sleep(500);

                // write b3dm from optimized gltf
                var glb = File.ReadAllBytes(glbname);
                var b3dm_new = new B3dm.Tile.B3dm(glb);
                var bytes = b3dm_new.ToBytes();
                File.WriteAllBytes(output_dir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(glbname) + ".b3dm", bytes);
            }

            // delete all glb
            System.Threading.Thread.Sleep(1000);
            var dir = new DirectoryInfo(output_dir);

            foreach (var file in dir.EnumerateFiles("*.glb"))
            {
                file.Delete();
            }
        }
    }
}
