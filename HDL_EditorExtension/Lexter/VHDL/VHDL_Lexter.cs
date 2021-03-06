﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;

using HDL_EditorExtension.Folding;
using HDL_EditorExtension.CodeCompletion;
using HDL_EditorExtension.CodeCompletion;

using VHDL;
using VHDL.parser;
using VHDL.parser.util;
using VHDL.statement;
using VHDL.annotation;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using Schematix.Core.Compiler;
using Schematix.Core.Model;


namespace HDL_EditorExtension.Lexter
{
    public class VHDL_Lexter:AbstractLexter
    {
        #region Propperties
        private VhdlFile file;
        public VhdlFile File
        {
            get { return file; }
            set 
            {
                lock (file)
                {
                    file = value;
                }
            }
        }

        /// <summary>
        /// Распарсеное дерево токенов
        /// </summary>
        private IParseTree tree;
        public IParseTree Tree
        {
            get { return tree; }
            set 
            {
                lock (tree)
                {
                    tree = value;
                }
            }
        }

        /// <summary>
        /// Используется как источник данных
        /// </summary>
        private CommonTokenStream tokenStream;
        public CommonTokenStream TokenStream
        {
            get { return tokenStream; }
            set { tokenStream = value; }
        }       

        public static readonly string[] ReservedWords = new string[] { "abs", "access", "after", "alias", "all", "and", "architecture", "array", "assert", "attribute",
                                            "begin", "block", "body", "buffer", "bus",
                                            "case", "component", "configuration", "constant",
                                            "disconnect", "downto",
                                            "else", "elsif", "end", "entity", "exit",
                                            "file", "for", "function",
                                            "generate", "generic", "group", "guarded",
                                            "if", "impure", "in", "inertial", "inout", "is",
                                            "label", "library", "linkage", "literal", "loop",
                                            "map", "mod",
                                            "nand", "new", "next", "nor", "not", "null",
                                            "of", "on", "open", "or", "others", "out",
                                            "package", "port", "postponed", "procedure", "process", "pure",
                                            "range", "record", "register", "reject", "rem", "report", "return", "rol", "ror",
                                            "select", "severity", "shared", "signal", "sla", "sll", "sra", "srl", "subtype",
                                            "then", "to", "transport", "type",
                                            "unaffected", "units", "until", "use",
                                            "variable",
                                            "wait", "when", "while", "with",
                                            "xnor", "xor" };
        #endregion

        private VHDL_CodeFile codeFile;
        public override CodeFile Code
        {
            get { return codeFile; }
        }

        private TextEditorExtention extendedTextEditor;

        public VHDL_Lexter(TextEditorExtention textEditor)
            : base(textEditor)
        {
            UpdateCompiler();
            this.extendedTextEditor = textEditor;
        }

        protected override void UpdateCompiler()
        {
            file = null;
            tree = null;
            tokenStream = null;

            //IndentationStrategy = new VHDLIndentationStrategy(textEditor.Options, this);
            indentationStrategy = new DefaultIndentationStrategy();
            FoldingStrategy = new VHDLFoldingStrategy(this);
            UpdateFolding();
            CodeCompletionList = new VHDLCodeCompletionList(textEditor.TextArea, this);
        }

        public override void Update(string text)
        {
            if ((Compiler != null) && (Compiler is VHDLCompiler))
            {
                VHDLCompiler compiler = Compiler as VHDLCompiler;
                ErrorList.Clear();

                codeFile = compiler.GetFileByPath(extendedTextEditor.FilePath) as VHDL_CodeFile;
                if (codeFile == null)
                {
                    codeFile = compiler.AddCodeFile(extendedTextEditor.FilePath) as VHDL_CodeFile;
                }
                if (codeFile != null)
                {
                    codeFile.Text = text;
                    compiler.ProcessCodeFile(codeFile);
                    file = codeFile.File;
                    
                    tokenStream = codeFile.TokenStream;
                    tree = codeFile.Tree;

                    if (codeFile.ParseSyntaxException != null)
                    {
                        //foreach (RecognitionException err in codeFile.SyntaxErrors.Errors)
                        //{
                            int offset = codeFile.ParseSyntaxException.OffendingSymbol.StartIndex;
                            int length = codeFile.ParseSyntaxException.OffendingSymbol.StopIndex - offset + 1;
                            Exception_Information inf = new Exception_Information(offset, length, codeFile.ParseSyntaxException.Message);
                            ErrorList.Add(inf);
                        //}
                    }
                    if (codeFile.ParseSemanticException != null)
                    {
                        //foreach (ParseError err in codeFile.SemanticErrors.Errors)
                        //{
                            int offset = codeFile.ParseSemanticException.Context.Start.StartIndex;
                            int length = codeFile.ParseSemanticException.Context.Start.StopIndex - offset + 1;
                            Exception_Information inf = new Exception_Information(offset, length, codeFile.ParseSemanticException.Message);
                            ErrorList.Add(inf);
                        //}
                    }
                    
                }
            }
        }

        public override UIElement GetDefinitionForWord(int Offset, string text)
        {
            return null;
        }

        public override void RenderData()
        {
            base.RenderData();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private IIndentationStrategy indentationStrategy;
        public override IIndentationStrategy IndentationStrategy
        {
            get
            {
                return indentationStrategy;
            }
            set 
            {
                indentationStrategy = value;
            }
        }

        private VHDLFoldingStrategy foldingStrategy;
        public override Folding.AbstractFoldingStrategy FoldingStrategy
        {
            get
            {
                return foldingStrategy;
            }
            set
            {
                if (value is VHDLFoldingStrategy)
                {
                    foldingStrategy = value as VHDLFoldingStrategy;
                    UpdateFolding();
                }
            }
        }

        private VHDLCodeCompletionList codeCompletionList;
        public override CodeCompletionList CodeCompletionList
        {
            get
            {
                return codeCompletionList;
            }
            set
            {
                if (value is VHDLCodeCompletionList)
                    codeCompletionList = value as VHDLCodeCompletionList;
            }
        }

        /// <summary>
        /// Найти элемент дерева по его номеру строки
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public IToken GetTreeElementByOffset(DocumentLine line)
        {
            if (Tree != null)
            {
                
            }
            return null;
        }

        /// <summary>
        /// Найти элемент дерева по его смещению
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public IToken GetTreeElementByOffset(int offset)
        {
            
            return null;
        }

        /// <summary>
        /// Получение элемента семантического дерева по его смещению
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public VhdlElement GetElementByOffset(VhdlElement parent, int offset)
        {
            if ((parent == null) || (file == null))
                return null;

            if (((parent is IDeclarativeRegion) == false) && ((parent is SequentialStatement) == false))
                return parent;

            if (parent is IDeclarativeRegion)
            {
                IDeclarativeRegion decl = parent as IDeclarativeRegion;
                List<object> objects = decl.Scope.GetLocalListOfObjects();
                if (objects.Count >= 1)
                {
                    foreach (object obj in objects)
                    {
                        if (parent == obj)
                            continue;
                        if (obj is VhdlElement)
                        {
                            PositionInformation pos = GetPositionInformation(obj as VhdlElement);
                            if ((pos != null) && (pos.Begin.Index <= offset) && (pos.End.Index >= offset))
                            {
                                (obj as VhdlElement).Parent = (parent as IDeclarativeRegion);
                                return GetElementByOffset(obj as VhdlElement, offset);
                            }
                            if (pos == null)
                            {
                                (obj as VhdlElement).Parent = (parent as IDeclarativeRegion);
                                return (obj as VhdlElement);
                            }
                        }
                    }
                }
            }
            if (parent is SequentialStatement)
            {
                SequentialStatement stat = parent as SequentialStatement;
                List<VhdlElement> objects = stat.GetAllStatements();
                if (objects.Count >= 1)
                {
                    foreach (VhdlElement obj in objects)
                    {
                        if (parent == obj)
                            continue;
                        if (obj == null)
                            continue;

                        PositionInformation pos = GetPositionInformation(obj as VhdlElement);
                        if ((pos != null) && (pos.Begin.Index <= offset) && (pos.End.Index >= offset))
                        {
                            (obj as VhdlElement).Parent = ((parent as SequentialStatement).Parent);
                            return GetElementByOffset(obj as VhdlElement, offset);
                        }
                        if (pos == null)
                        {
                            (obj as VhdlElement).Parent = (parent as IDeclarativeRegion);
                            return (obj as VhdlElement);
                        }
                    }
                }
            }
            return parent;
        }

        /// <summary>
        /// Получение информации о позиции в коде элемента
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public PositionInformation GetPositionInformation(VhdlElement parent)
        {
            return Annotations.getAnnotation<PositionInformation>(parent);            
        }
    }
}
