using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Schematix.CommonProperties
{
    [Serializable()]
    public class ProjectOptions : IOptions, ISerializable
    {
        /// <summary>
        /// Папка с проектами пользователя, заданная по-умолчанию
        /// </summary>
        private string defaultProjectLocation;
        public string DefaultProjectLocation
        {
            get
            {
                return defaultProjectLocation;
            }
            set
            {
                if (System.IO.Directory.Exists(value) == false)
                    throw new Exception("Invalid path");
                else
                    defaultProjectLocation = value;
            }
        }

        #region constructors
        public ProjectOptions()
        {
        }
        public ProjectOptions(SerializationInfo info, StreamingContext ctxt)
        {
            this.defaultProjectLocation = (string)info.GetValue("DefaultProjectLocation", typeof(string));
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
            defaultProjectLocation = System.IO.Path.Combine
                    (
                        System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "HDL_Light"
                    ); ;
        }

        #endregion

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("DefaultProjectLocation", DefaultProjectLocation);
        }

        #endregion
    }
}
