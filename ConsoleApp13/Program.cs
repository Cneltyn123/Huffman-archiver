using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    class Node
    {
        public byte Simv { get; set; }
        public int Frequency { get; set; }
        public Node Right { get; set; }
        public Node Left { get; set; }

        public List<bool> Traverse(byte symbol, List<bool> data)
        {

            if (Right == null && Left == null)
            {
                if (symbol.Equals(this.Simv))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<bool> left = null;
                List<bool> right = null;

                if (Left != null)
                {
                    List<bool> leftPath = new List<bool>();
                    leftPath.AddRange(data);
                    leftPath.Add(false);

                    left = Left.Traverse(symbol, leftPath);
                }

                if (Right != null)
                {
                    List<bool> rightPath = new List<bool>();
                    rightPath.AddRange(data);
                    rightPath.Add(true);
                    right = Right.Traverse(symbol, rightPath);
                }

                if (left != null)
                {
                    return left;
                }
                else
                {
                    return right;
                }
            }
        }
    }
    class HuffmanDerevo
    {
        private List<Node> nodes = new List<Node>();
        public Node Root { get; set; }
        public Dictionary<byte, int> Frequencies = new Dictionary<byte, int>();     //встречаемость

        public void Build(byte[] source)        //построение дерева хаффмана 
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (!Frequencies.ContainsKey(source[i]))
                {
                    Frequencies.Add(source[i], 0);
                }

                Frequencies[source[i]]++;   //подсчитали встречаемость
            }

            foreach (KeyValuePair<byte, int> symbol in Frequencies)
            {
                nodes.Add(new Node() { Simv = symbol.Key, Frequency = symbol.Value }); //записали встречаемость и символ
            }

            while (nodes.Count > 1)
            {
                List<Node> ordered = nodes.OrderBy(node => node.Frequency).ToList<Node>();

                if (ordered.Count >= 2)
                {
                    List<Node> taken = ordered.Take(2).ToList<Node>();
                    Node parent = new Node()        // объединение двух частот 
                    {
                        Simv = 255,
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };
                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }
                this.Root = nodes.FirstOrDefault();
            }
        }
        public BitArray Encode(byte[] source)
        {
            List<bool> encoded = new List<bool>();
            byte sym;
            // BinaryReader fin = new BinaryReader(new FileStream(input, FileMode.Open));
            string output = @"C:\Users\sasha\Downloads\hslov.txt";
            BinaryWriter dest = new BinaryWriter(new FileStream(output, FileMode.Create));
            Dictionary<byte, List<bool>> dict = new Dictionary<byte, List<bool>>(); // словарь(символ) -> код символа

            int l = Frequencies.Count;
            dest.Write(l); // пишем, сколько всего раных символов
            List<bool> encodedSymbol = new List<bool>(); // код символа
            foreach (KeyValuePair<byte, int> symbol in Frequencies) // записываем "таблицу" символов
            {
                encodedSymbol = this.Root.Traverse(symbol.Key, new List<bool>()); // массив из 0 и 1 - код символа

                dest.Write(symbol.Key); // пишем сам символ
                dest.Write(encodedSymbol.Count); // количество бит в коде
                foreach (bool b in encodedSymbol) // пишем сам код
                {
                    dest.Write(b);
                }
                dict.Add(symbol.Key, encodedSymbol);

            }
            long s = dest.BaseStream.Length;

            for (int i = 0; i < source.Length; i++)
            {
                List<bool> encodedSim = this.Root.Traverse(source[i], new List<bool>());
                encoded.AddRange(encodedSim);
                
            }
            dest.Close();
            BitArray bits = new BitArray(encoded.ToArray());
           
          
            return bits;
        }
        public static byte[] ToByteArray(BitArray bits)     //двоичный код в байты
        {
            const int BYTE = 8;
            int length = (bits.Count / BYTE) + ((bits.Count % BYTE == 0) ? 0 : 1);
            var bytes = new byte[length];

            for (int i = 0; i < bits.Length; i++)
            {

                int bitIndex = i % BYTE;
                int byteIndex = i / BYTE;

                int mask = (bits[i] ? 1 : 0) << bitIndex;
                bytes[byteIndex] |= (byte)mask;
            }

            return bytes;
        }
        public string Decode(BitArray bits1)
        {
            string output = @"C:\Users\sasha\Downloads\hslov.txt";
            BinaryReader bits = new BinaryReader(new FileStream(output, FileMode.Open));
            BinaryWriter fout1 = new BinaryWriter(new FileStream(@"C:\Users\sasha\Downloads\hdeco.txt", FileMode.Create));
            Dictionary<int, byte> dict = new Dictionary<int, byte>(); // словарь(хэш)->символ // {123: "a", 228: "b", 1488:"c"}

            int n = bits.ReadInt32(); // колличество различных символов
                                      //byte min = 255;
            Dictionary<int, List<bool>> d = new Dictionary<int, List<bool>>();
            Dictionary<List<bool>, int> y = new Dictionary<List<bool>, int>();

            for (int i = 0; i < n; i++) // считываем таблицу символ-код
            {
                int code = 0;
                byte sym = bits.ReadByte(); // считываем символ            
                int cnt = bits.ReadInt32(); // считывает длину в битах
                bool p;
                List<bool> kod = new List<bool>();
                //min = (min > cnt) ? cnt : min;
                for (int j = 0; j < cnt; j++) // полуаем 10-е число из 2-го
                {
                    p = bits.ReadBoolean();
                    kod.Add(p);
                    code += (p) ? 1 : 0;
                }
                d.Add(sym, kod);
                y.Add(kod, sym);
                //if (!dict.ContainsKey(code + pow2[(cnt + 1) / 2]))
                //    dict.Add(code + pow2[(cnt + 1)/2], sym); // хэшируем и добавляем в словарь
                //   if (!dict.ContainsKey(code))
                //dict.Add(code, sym); // хэшируем и добавляем в словарь
            }

            Node current = this.Root;
            StringBuilder sb = new StringBuilder();
            List<bool> pp = new List<bool>();
            //foreach (KeyValuePair<List<bool>, int> kvp in y)
            //{
            //    foreach (bool bit in bits1)
            //    {
            //        List<bool> j = kvp.Key;
            //        pp.Add(bit);
            //        if (pp == kvp.Key) { sb.Append(kvp.Value); pp.Clear(); }
            //    }

            //}
            List<byte> s = new List<byte>();
            foreach (bool bit in bits1)
            {
                pp.Add(bit);
                //  if (y.ContainsKey(pp.ToList())) { sb.Append(y[pp].ToString()); pp.Clear(); }
                foreach (KeyValuePair<List<bool>, int> kvp in y)
                {
                    List<bool> j = kvp.Key;
                    bool isEqual = j.SequenceEqual(pp);
                    if (isEqual) {
                        s.Add((byte)kvp.Value);
                      //   char t=(char)kvp.Value ;
                       // fout1.Write( kvp.Value );
                       //  sb.Append(( kvp.Value ));
                        pp.Clear(); }
                   // if (pp == kvp.Key) { sb.Append(kvp.Value); pp.Clear(); }
                }
                // if (d.ContainsValue(pp))
                //  if (y.TryGetValue(pp, out int c))
                //  { sb.Append(c.ToString()); pp.Clear(); }
                //if (bit)
                //{
                //    if (current.Right != null)
                //    {
                //        current = current.Right;
                //    }
                //}
                //else
                //{
                //    if (current.Left != null)
                //    {
                //        current = current.Left;
                //    }
                //}

                //if (IsLeaf(current)) // если нет ветвей, пишем 
                //{
                //    sb.Append(current.Simv);
                //    current = this.Root;
                //}

            }
            byte[] ss = s.ToArray();
            File.WriteAllBytes(@"C:\Users\sasha\Downloads\hdee.txt", ss);
            return sb.ToString();
        }

        public bool IsLeaf(Node node)
        {
            return (node.Left == null && node.Right == null);
        }
    }
    class Program
    {
        public static byte[] ToByteArray(BitArray bits)     //двоичный код в байты
        {
            const int BYTE = 8;
            int length = (bits.Count / BYTE) + ((bits.Count % BYTE == 0) ? 0 : 1);
            var bytes = new byte[length];

            for (int i = 0; i < bits.Length; i++)
            {

                int bitIndex = i % BYTE;
                int byteIndex = i / BYTE;

                int mask = (bits[i] ? 1 : 0) << bitIndex;
                bytes[byteIndex] |= (byte)mask;
            }

            return bytes;
        }
        static void Main(string[] args)
        {
            string input = "";
            StreamReader sr = new StreamReader(@"C:\Users\sasha\Downloads\voina.txt", Encoding.Default);
            while (sr.Peek() != -1)
            {
                input = sr.ReadToEnd();
            }
            Console.WriteLine("считали");

            HuffmanDerevo huffmanTree = new HuffmanDerevo();
            byte[] array = File.ReadAllBytes(@"C:\Users\sasha\Downloads\voina.txt"); 
            huffmanTree.Build(array);
            BitArray encoded = huffmanTree.Encode(array);
            //using (StreamWriter sw = new StreamWriter(@"C:\Users\sasha\Downloads\henc.txt", false, System.Text.Encoding.Default))
            //{
            //    sw.Write(ToByteArray(encoded));
            //}
                using (StreamWriter sw = new StreamWriter(@"C:\Users\sasha\Downloads\huffencoded.txt", false, System.Text.Encoding.Default))
            {
                foreach (bool bit in encoded)
                {
                    sw.Write((bit ? 1 : 0) + "");
                }
            }
            Console.WriteLine("записали Хафф закодированный ");
            Console.WriteLine();

            File.WriteAllBytes(@"C:\Users\sasha\Downloads\output_file.txt", ToByteArray(encoded));
            Console.WriteLine("записали Хафф закодированный байтовый ");

            string decoded = huffmanTree.Decode(encoded);

            File.WriteAllText(@"C:\Users\sasha\Downloads\Hufdec.txt", decoded, Encoding.Default);
            Console.WriteLine("записали Хафф декод ");

            FileInfo file = new FileInfo(@"C:\Users\sasha\Downloads\Hufdec.txt");
            double size = file.Length;
            FileInfo file1 = new FileInfo(@"C:\Users\sasha\Downloads\output_file.txt");
            double size1 = file1.Length;
            Console.WriteLine("Коэффициент сжатия " + ((1 - (size1 / size)) * 100).ToString());

            Console.ReadKey();
        }
    }
}
