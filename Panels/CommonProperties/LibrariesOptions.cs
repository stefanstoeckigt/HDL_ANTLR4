using System;
using System.Collections.Generic;
using System.Text;


using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Schematix.CommonProperties
{
    [Serializable()]
    public class LibrariesOptions :IOptions, ISerializable
    {
        /// <summary>
        /// Список папок с библиоткей Verilog
        /// </summary>
        private List<string> verilogLibrariesPaths;
        public List<string> VerilogLibrariesPaths
        {
            get
            {
                return verilogLibrariesPaths;
            }
            set
            {
                verilogLibrariesPaths.Clear();
                foreach (string path in value)
                {
                    if (System.IO.Directory.Exists(path) == true)
                        verilogLibrariesPaths.Add(path);
                }
            }
        }

        /// <summary>
        /// Список папок с библиоткей VHDL
        /// </summary>
        private List<string> _VHDLLibrariesPaths;
        public List<string> VHDLLibrariesPaths
        {
            get
            {
                return _VHDLLibrariesPaths;
            }
            set
            {
                _VHDLLibrariesPaths.Clear();
                foreach (string path in value)
                {
                    if (System.IO.Directory.Exists(path) == true)
                        _VHDLLibrariesPaths.Add(path);
                }
            }
        }

        /// <summary>
        /// Путь к файлу с настройками библиотек
        /// </summary>
        public string LibraryConfigurationPath
        {
            get { return System.IO.Path.Combine(CommonProperties.Configuration.CurrentConfiguration.ProjectOptions.DefaultProjectLocation, "LibraryRepository.xml"); }
        }
        

        #region constructors
        public LibrariesOptions()
        {
        }
        public LibrariesOptions(SerializationInfo info, StreamingContext ctxt)
        {
            this.verilogLibrariesPaths = (List<string>)info.GetValue("VerilogLibrariesPaths", typeof(List<string>));
            this._VHDLLibrariesPaths =   (List<string>)info.GetValue("VHDLLibrariesPaths",    typeof(List<string>));
        }
        #endregion

        #region IOptions Members

        public void LoadData(Options options)
        {
            options.SetOptionsData(this);
        }

        public void Accept(Options options)
        {
            options.GetOptionsData(this);
        }

        public void SetDefault()
        {
            verilogLibrariesPaths = new List<string>();
            _VHDLLibrariesPaths = new List<string>();

            string CurrentPath = System.IO.Directory.GetCurrentDirectory();
            string DefLibPath = System.IO.Path.Combine(CurrentPath, "Libraries");
            _VHDLLibrariesPaths.Add(DefLibPath);
        }

        #endregion

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("VerilogLibrariesPaths", VerilogLibrariesPaths);
            info.AddValue("VHDLLibrariesPaths", VHDLLibrariesPaths);
        }

        #endregion
    }
}
