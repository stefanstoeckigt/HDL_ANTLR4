using System;
using System.Collections.Generic;
using System.Text;


using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Schematix.CommonProperties
{
    [Serializable()]
    public class GraphicsOptions: ISerializable
    {
                /// <summary>
        /// Отображать ли сетку
        /// </summary>
        private bool showGrid;
        public bool ShowGrid
        {
            get
            {
                return showGrid;
            }
            set
            {
                showGrid = value;
            }
        }

        /// <summary>
        /// Отображать ли границу
        /// </summary>
        private bool showBorder;
        public bool ShowBorder
        {
            get
            {
                return showBorder;
            }
            set
            {
                showBorder = value;
            }
        }

        /// <summary>
        /// Цвет фона
        /// </summary>
        private System.Drawing.Color bgColor;
        public System.Drawing.Color BGColor
        {
            get
            {
                return bgColor;
            }
            set
            {
                bgColor = value;
            }
        }

        /// <summary>
        /// Цвет границы
        /// </summary>
        private System.Drawing.Color borderColor;
        public System.Drawing.Color BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                borderColor = value;
            }
        }

        /// <summary>
        /// Цвет выделения
        /// </summary>
        private System.Drawing.Color selectColor;
        public System.Drawing.Color SelectColor
        {
            get
            {
                return selectColor;
            }
            set
            {
                selectColor = value;
            }
        }

        #region constructors
        public GraphicsOptions()
        {
        }
        public GraphicsOptions(SerializationInfo info, StreamingContext ctxt)
        {
            this.showBorder = (bool)info.GetValue("ShowBorder", typeof(bool));
            this.showGrid   = (bool)info.GetValue("ShowGrid", typeof(bool));
            this.borderColor = (System.Drawing.Color)info.GetValue("BorderColor", typeof(System.Drawing.Color));
            this.bgColor     = (System.Drawing.Color)info.GetValue("BGColor",     typeof(System.Drawing.Color));
            this.selectColor = (System.Drawing.Color)info.GetValue("SelectColor", typeof(System.Drawing.Color));
        }
        #endregion

        public virtual void SetDefault()
        {
            showGrid = true;
            showBorder = true;
            bgColor = System.Drawing.Color.White;
            borderColor = System.Drawing.Color.Black;
            selectColor = System.Drawing.Color.Blue;
        }

        #region ISerializable Members

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ShowBorder",  ShowBorder);
            info.AddValue("ShowGrid",    ShowGrid);
            info.AddValue("BorderColor", BorderColor);
            info.AddValue("BGColor",     BGColor);
            info.AddValue("SelectColor", SelectColor);
        }

        #endregion
    }
}
