using System;
using System.Collections.Generic;
using System.IO;


namespace mbac2obj
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(args[0])))
                {
                    string magic = new string(br.ReadChars(2));
                    if (magic != "MB")
                        return;

                    Console.WriteLine("#mbac2obj by Durik256/ orig python script: by minexew");

                    int[] MAGNITUDE = new int[] { 8, 10, 13, 16 };
                    Vector3[] DIRECTION = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 0, -1)};

                    List<Vector3> vertices = new List<Vector3>();
                    Vector3[] normals = null;
                    Vector3[] uvs = null;
                    List<int> faces = new List<int>();

                    int ver = br.ReadUInt16();
                    byte vformat = br.ReadByte();
                    byte nformat = br.ReadByte();
                    byte pformat = br.ReadByte();
                    byte bformat = br.ReadByte();

                    ushort num_vertices = br.ReadUInt16();
                    ushort num_polyt3 = br.ReadUInt16();
                    ushort num_polyt4 = br.ReadUInt16();
                    ushort num_bones = br.ReadUInt16();

                    ushort num_polyf3 = br.ReadUInt16();
                    ushort num_polyf4 = br.ReadUInt16();
                    ushort matcnt = br.ReadUInt16();
                    ushort unk21 = br.ReadUInt16();
                    ushort num_color = br.ReadUInt16();

                    for (int i = 0; i < unk21; i++)
                    {
                        br.BaseStream.Seek(4, SeekOrigin.Current);
                        for (int j = 0; j < matcnt; j++)
                            br.BaseStream.Seek(4, SeekOrigin.Current);
                    }

                    //decode vertices
                    Unpacker unp = new Unpacker(br);

                    while (vertices.Count < num_vertices)
                    {
                        int header = unp.uBit(8);
                        int magnitude = MAGNITUDE[header >> 6];
                        int count = (header & 0x3F) + 1;

                        for (int i = 0; i < count; i++)
                        {
                            vertices.Add(new Vector3(unp.Bit(magnitude), unp.Bit(magnitude), unp.Bit(magnitude)));
                        }
                    }

                    //decode normals
                    if (nformat != 0) {
                        normals = new Vector3[num_vertices];
                        unp = new Unpacker(br);

                        int have_normals = 0;
                        while (have_normals < num_vertices)
                        {
                            float x = unp.Bit(7);
                            if (x == -64)
                            {
                                int direction = unp.uBit(3);
                                normals[have_normals] = DIRECTION[direction];
                            }
                            else
                            {
                                x = x / 64;
                                float y = (float)unp.Bit(7) / 64;
                                int z_negative = unp.uBit(1);

                                float z = 0;
                                if (1 - x * x - y * y >= 0)
                                    z = (float)Math.Sqrt(1 - x * x - y * y) * ((z_negative != 0) ? -1 : 1);
                                else
                                    z = 0;
                                normals[have_normals] = new Vector3(x, y, z);
                            }
                            have_normals += 1;
                        }
                    }
                    
                    //decode polygons
                    unp = new Unpacker(br);
                    if (num_polyf3 + num_polyf4 > 0)
                    {
                        int unk_bits = unp.uBit(8);
                        int vertex_index_bits = unp.uBit(8);
                        int color_bits = unp.uBit(8);
                        int color_id_bits = unp.uBit(8);
                        int ubit = unp.uBit(8);

                        for (int i = 0; i < num_color; i++)
                        {
                            int r = unp.uBit(color_bits);
                            int g = unp.uBit(color_bits);
                            int b = unp.uBit(color_bits);
                        }
                        
                        for (int i = 0; i < num_polyf3; i++) {
                            int unknown = unp.uBit(unk_bits);
                            int a = unp.uBit(vertex_index_bits);
                            int b = unp.uBit(vertex_index_bits);
                            int c = unp.uBit(vertex_index_bits);
                            int color_id = unp.uBit(color_id_bits);
                            faces.AddRange(new int[] { c, b, a });
                        }

                        for (int i = 0; i < num_polyf4; i++) {
                            int unknown = unp.uBit(unk_bits);
                            int a = unp.uBit(vertex_index_bits);
                            int b = unp.uBit(vertex_index_bits); 
                            int c = unp.uBit(vertex_index_bits); 
                            int d = unp.uBit(vertex_index_bits);

                            int color_id = unp.uBit(color_id_bits);
                            faces.AddRange(new int[] { c, b, a, d, b, c });
                        }

                    }
                    
                    if(num_polyt3 + num_polyt4 > 0)
                    {
                        uvs = new Vector3[num_vertices];
                        int unk_bits = unp.uBit(8);
                        int vertex_index_bits = unp.uBit(8);
                        int uv_bits = unp.uBit(8);
                        int somedata = unp.uBit(8);

                        for (int i = 0; i < num_polyt3; i++)
                        {
                            int unknown = unp.uBit(unk_bits);
                            int a = unp.uBit(vertex_index_bits);
                            int b = unp.uBit(vertex_index_bits);
                            int c = unp.uBit(vertex_index_bits);
                            int[] uv = new int[6];
                            for (int j = 0; j < 6; j++)
                                uv[j] = unp.uBit(uv_bits);

                            uvs[a] = new Vector3(uv[0], uv[1], 0);
                            uvs[b] = new Vector3(uv[2], uv[3], 0);
                            uvs[c] = new Vector3(uv[4], uv[5], 0);

                            faces.AddRange(new int[] { c, b, a });
                        }

                        for (int i = 0; i < num_polyt4; i++)
                        {
                            int unknown = unp.uBit(unk_bits);
                            int a = unp.uBit(vertex_index_bits);
                            int b = unp.uBit(vertex_index_bits);
                            int c = unp.uBit(vertex_index_bits);
                            int d = unp.uBit(vertex_index_bits);
                            int[] uv = new int[8];
                            for (int j = 0; j < 8; j++)
                                uv[j] = unp.uBit(uv_bits);


                            uvs[a] = new Vector3(uv[0], uv[1], 0);
                            uvs[b] = new Vector3(uv[2], uv[3], 0);
                            uvs[c] = new Vector3(uv[4], uv[5], 0);
                            uvs[d] = new Vector3(uv[6], uv[7], 0);

                            faces.AddRange(new int[] { c, b, a, d, b, c });
                        }
                    }

                    saveOBJ(Path.ChangeExtension(args[0], ".obj"), vertices, normals, uvs, faces);
                    //-----------
                }
            }
        }

        static void saveOBJ(string outPath, List<Vector3> vert, Vector3[] norm, Vector3[] uvs, List<int> face)
        {
            using (StreamWriter writer = new StreamWriter(outPath))
            {
                writer.WriteLine("#mbac2obj by Durik256");
                //writer.WriteLine($"mtllib {Path.GetFileName(Path.ChangeExtension(outPath, ".mtl"))}");

                for (int i = 0; i < vert.Count; i++)
                    writer.WriteLine($"v {vert[i].X} {vert[i].Y} {vert[i].Z} ".Replace(',', '.'));

                if (norm != null) {
                    for (int i = 0; i < norm.Length; i++)
                        writer.WriteLine($"vn {norm[i].X} {norm[i].Y} {norm[i].Z} ".Replace(',', '.'));
                }

                if (uvs != null) {
                    for (int i = 0; i < uvs.Length; i++)
                        writer.WriteLine($"vt {uvs[i].X} {uvs[i].Y} ".Replace(',', '.'));
                }

                for (int i = 0; i < face.Count; i += 3)
                {
                    int a = face[i] + 1;
                    int b = face[i + 1] + 1;
                    int c = face[i + 2] + 1;
                    if(uvs != null && norm != null)
                        writer.WriteLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
                    if (uvs != null && norm == null)
                        writer.WriteLine($"f {a}/{a} {b}/{b} {c}/{c}");
                    if (uvs == null && norm != null)
                        writer.WriteLine($"f {a}//{a} {b}//{b} {c}//{c}");
                }
            }
        }
    }

    class Unpacker
    {
        BinaryReader br;
        int havebits;
        int data;

        public Unpacker(BinaryReader br)
        {
            this.br = br;
        }

        public int uBit(int nbits)
        {
            while (nbits > havebits)
                addbits();

            int bits = data & ((1 << nbits) - 1);
            havebits -= nbits;
            data >>= nbits;
            return bits;
        }

        public int Bit(int nbits) {
            int value = uBit(nbits);
            int sign = value & (1 << (nbits - 1));

            if (sign != 0)
                value -= (1 << nbits);
            return value;
        }

        void addbits()
        {
            data |= br.ReadByte() << havebits;
            havebits += 8;
        }
    }

    class Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
        public Vector3()
        {
            X = 0; Y = 0; Z = 0;
        }
    }
}
