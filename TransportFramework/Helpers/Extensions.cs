namespace TransportFramework.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;

    using Parsers.Base;
    using Transports.Base;

    /// <summary>
    /// The extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sum of a Byte array.
        /// </summary>
        /// <param name="data"> The data. </param>
        /// <returns> The <see cref="uint"/>. </returns>
        public static uint Sum(this IEnumerable<byte> data)
        {
            return (uint)data.Aggregate(0, (current, b) => current + b);
        }

        /// <summary>
        /// The get first Parser by type.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <typeparam name="T">
        /// The type of Parser to Find.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T GetParserType<T>(this TransportBase source) where T : Parser
        {
            return source.GetParserByType<T>().FirstOrDefault();
        }

        /// <summary>
        /// The get parser by type.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <typeparam name="T">
        /// The type of Parser to Find.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static List<T> GetParserByType<T>(this TransportBase source) where T : Parser
        {
            return source.Parsers.Where(x => x.GetType() == typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// The chunk.
        /// </summary>
        /// <typeparam name="T">
        /// The generic
        /// </typeparam>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="chunkSize">
        /// The chunk size.
        /// </param>
        /// <returns>
        /// The <see cref="Enumerable"/>.
        /// </returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var chunk = new List<IEnumerable<T>>();
            while (source.Any())
            {
                chunk.Add(source.Take(chunkSize));
                source = source.Skip(chunkSize);
            }
            // ReSharper restore PossibleMultipleEnumeration
            return chunk;
        }

        /// <summary>
        /// [Extension] Converts an enumeration value to its byte representation.
        /// </summary>
        /// <param name="value">The enumeration value to Convert</param>
        /// <returns>The byte value of an enumeration</returns>
        public static byte ToByte(this Enum value)
        {
            return Convert.ToByte(value);
        }

        /// <summary>
        /// The wait.
        /// </summary>
        /// <param name="ms">
        /// The milliseconds to wait.
        /// </param>
        public static void Wait(int ms)
        {
            var sp = Stopwatch.StartNew();
            while (sp.ElapsedMilliseconds < ms)
            {
            }
        }

        /// <summary>
        /// The wait.
        /// </summary>
        /// <param name="ms">
        /// The milliseconds to wait.
        /// </param>
        /// <param name="func">
        /// The function
        /// </param>
        public static void Wait(int ms, Func<bool> func)
        {
            var sp = Stopwatch.StartNew();
            while (sp.ElapsedMilliseconds < ms)
            {
                if (!func())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// The to Enum.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// The Type of Enum
        /// </typeparam>
        /// <returns>
        /// The value as an Enum
        /// </returns>
        public static T ToEnum<T>(this int value)
        {
            if (Enum.IsDefined(typeof(T), value))
            {
                return (T)Enum.Parse(typeof(T), value.ToString(CultureInfo.InvariantCulture));
            }

            return default(T);
        }

        /// <summary>
        /// The to enum.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// The Type of enum
        /// </typeparam>
        /// <returns>
        /// The value as an Enum
        /// </returns>
        public static T ToEnum<T>(this byte value)
        {
            return (T)Enum.Parse(typeof(T), value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The to enum.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        ///  The Type of enum
        /// </typeparam>
        /// <returns>
        /// The value as an Enum
        /// </returns>
        public static T ToEnum<T>(this string value)
        {
            return ToEnum<T>(int.Parse(value));
        }

        /// <summary>
        /// The index of last.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="match">
        /// The match.
        /// </param>
        /// <typeparam name="T">
        /// The type of list
        /// </typeparam>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int IndexOfLast<T>(this IList<T> source, Predicate<T> match)
        {
            for (var i = source.Count - 1; i >= 0; i--)
            {
                if (match(source[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// The index of first.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="match">
        /// The match.
        /// </param>
        /// <typeparam name="T">
        /// the type of list
        /// </typeparam>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int IndexOfFirst<T>(this IList<T> source, Predicate<T> match)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (match(source[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// The raise event.
        /// </summary>
        /// <param name="ev">
        /// The event.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <typeparam name="T">
        /// the type of event.
        /// </typeparam>
        public static void RaiseEvent<T>(this EventHandler<T> ev, object sender, T args) where T : EventArgs
        {
            ev?.Invoke(sender, args);
        }

        /// <summary>
        /// The raise event.
        /// </summary>
        /// <param name="ev">
        /// The event.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <typeparam name="T">
        /// the type of event.
        /// </typeparam>
        public static void RaiseEventStruct<T>(this EventHandler<T> ev, object sender, T args) where T : struct
        {
            ev?.Invoke(sender, args);
        }

        /// <summary>
        /// The raise event.
        /// </summary>
        /// <param name="ev">
        /// The event.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <typeparam name="T">
        /// the type of event.
        /// </typeparam>
        public static void RaiseEventClass<T>(this EventHandler<T> ev, object sender, T args) where T : class
        {
            ev?.Invoke(sender, args);
        }

        /// <summary>
        /// The raise event.
        /// </summary>
        /// <param name="ev">
        /// The event.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <typeparam name="T">
        /// the type of event.
        /// </typeparam>
        public static void RaiseEvent<T>(this Delegate ev, object sender, T args) where T : EventArgs
        {
            var e = ev;
            e?.DynamicInvoke(sender, args);
        }

        /// <summary>
        /// The raise event.
        /// </summary>
        /// <param name="ev">
        /// The event.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <typeparam name="T">
        /// the type of event.
        /// </typeparam>
        public static void RaiseEventAsync<T>(this EventHandler<T> ev, object sender, T args) where T : EventArgs
        {
            var e = ev;
            if (e != null)
            {
                Task.Factory.StartNew(() => e(sender, args));
            }
        }

        /// <summary>
        /// The raise.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        public static void Raise<T, Y>(this EventHandler<KeyValuePair<T, Y>> e, object sender, KeyValuePair<T, Y> data)
        {
            e?.Invoke(sender, data);
        }

        /// <summary>
        /// The wait.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="canContinue">
        /// The can Continue.
        /// </param>
        /// <param name="retry">
        /// The retry.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool Wait(this object o, Action action, ref bool canContinue, int retry = 1, int timeout = 250)
        {
            canContinue = false;

            var end = DateTime.Now.AddMilliseconds(timeout);

            action();

            while (!canContinue)
            {
                if (end > DateTime.Now)
                {
                    continue;
                }

                if (--retry < 0)
                {
                    return false;
                }

                end = DateTime.Now.AddMilliseconds(timeout);

                if (!canContinue)
                {
                    action();
                }
            }

            return true;
        }

        /// <summary>
        /// The wait action.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="isTrue">
        /// The is true.
        /// </param>
        /// <param name="retry">
        /// The retry.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool Wait(this object o, Action action, Func<bool> isTrue, int retry = 1, int timeout = 250)
        {
            var end = DateTime.Now.AddMilliseconds(timeout);

            action();

            while (isTrue())
            {
                if (end > DateTime.Now)
                {
                    continue;
                }

                if (--retry < 0)
                {
                    return false;
                }

                if (!isTrue())
                {
                    continue;
                }

                end = DateTime.Now.AddMilliseconds(timeout);
                action();
            }

            return true;
        }

        /// <summary>
        /// The wait action.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="isTrue">
        /// The is true.
        /// </param>
        /// <param name="retry">
        /// The retry.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static async Task<bool> WaitAsync(this object o, Action action, Func<bool> isTrue, int retry = 1, int timeout = 250)
        {
            var end = DateTime.Now.AddMilliseconds(timeout);

            action();

            while (isTrue())
            {
                if (end > DateTime.Now)
                {
                    await Task.Delay(1);
                    continue;
                }

                if (--retry < 0)
                {
                    return false;
                }

                if (!isTrue())
                {
                    break;
                }

                end = DateTime.Now.AddMilliseconds(timeout);
                action();
            }

            return true;
        }

        /// <summary>
        /// The to structure.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T ToStructure<T>(this IEnumerable<byte> source, int offset = 0) where T : struct
        {
            return source.ToArray().ToStructure<T>(offset);
        }

        /// <summary>
        /// The to class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T ToClass<T>(this IEnumerable<byte> source, int offset = 0) where T : class
        {
            return source.ToArray().ToClass<T>(offset);
        }

        /// <summary>
        /// The byte array to structure.
        /// </summary>
        /// <param name="source">
        /// The source data.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T ToStructure<T>(this byte[] source, int offset = 0) where T : struct
        {
            var response = new T();

            var size = Marshal.SizeOf(response);
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(source, offset, ptr, size);

            response = (T)Marshal.PtrToStructure(ptr, response.GetType());

            Marshal.FreeHGlobal(ptr);

            return response;
        }

        /// <summary>
        /// The to class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T ToClass<T>(this byte[] source, int offset = 0)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(source, offset, source.Length - offset);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)new BinaryFormatter().Deserialize(ms);
            }
        }

        /// <summary>
        /// Convert a structure to a byte array.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static byte[] ToByteArray<T>(this T source)
        {
            if (typeof(T).IsClass)
            {
                if (source == null)
                {
                    return null;
                }

                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, source);
                    return ms.ToArray();
                }
            }

            var size = Marshal.SizeOf(source);
            var ptr = Marshal.AllocHGlobal(size);

            var response = new byte[size];

            Marshal.StructureToPtr(source, ptr, true);
            Marshal.Copy(ptr, response, 0, size);
            Marshal.FreeHGlobal(ptr);

            return response;
        }

        /// <summary>
        /// The load from resources.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool LoadFromResources(string tag)
        {
            var found = false;

            // try the calling assembly. 
            foreach (var file in Assembly.GetCallingAssembly().GetManifestResourceNames()
                .Where(x => x.ToLower().Contains(tag)))
            {
                var split = file.Split('.');
                var name = Path.GetFileName(split[split.Length - 2] + ".dll");

                if (File.Exists(name))
                {
                    found = true;
                    continue;
                }

                using (var ms = new FileStream(Path.GetFileName(name), FileMode.CreateNew))
                {
                    var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(file);
                    if (stream != null)
                    {
                        stream.CopyTo(ms);
                    }

                    found = true;
                }
            }

            if (found)
            {
                return true;
            }

            // try the execuing assembly
            foreach (var file in Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.ToLower().Contains(tag)))
            {
                var split = file.Split('.');
                var name = Path.GetFileName(split[split.Length - 2] + ".dll");

                if (File.Exists(name))
                {
                    found = true;
                    continue;
                }

                using (var ms = new FileStream(Path.GetFileName(name), FileMode.CreateNew))
                {
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file);
                    stream?.CopyTo(ms);

                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// The load from resource.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool LoadFromResource(string tag)
        {
            var found = false;

            // try the calling assembly. 
            foreach (var file in Assembly.GetCallingAssembly().GetManifestResourceNames()
                .Where(x => x.ToLower().Contains(tag)))
            {
                var split = file.Split('.');
                var name = split[split.Length - 2] + ".dll";

                using (var ms = new FileStream(Path.GetFileName(name), FileMode.CreateNew))
                {
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file);
                    stream?.CopyTo(ms);

                    found = true;
                    break;
                }
            }

            if (found)
            {
                return true;
            }

            // try the execuing assembly
            foreach (var file in Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.ToLower().Contains(tag)))
            {
                var split = file.Split('.');
                var name = split[split.Length - 2] + ".dll";

                using (var ms = new FileStream(Path.GetFileName(name), FileMode.CreateNew))
                {
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file);
                    stream?.CopyTo(ms);

                    found = true;
                    break;
                }
            }

            return found;
        }
    }
}
