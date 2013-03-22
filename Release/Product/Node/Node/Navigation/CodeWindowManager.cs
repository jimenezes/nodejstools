﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.PythonTools.Navigation {
    class CodeWindowManager : IVsCodeWindowManager {
        private readonly IVsCodeWindow _window;
        private readonly IWpfTextView _textView;
        private static readonly Dictionary<IWpfTextView, CodeWindowManager> _windows = new Dictionary<IWpfTextView, CodeWindowManager>();
#if FALSE
        private readonly EditFilter _filter;
        private DropDownBarClient _client;
#endif

        public CodeWindowManager(IVsCodeWindow codeWindow, IWpfTextView textView) {
            _window = codeWindow;
            _textView = textView;

#if FALSE
            var model = PythonToolsPackage.ComponentModel;
            var adaptersFactory = model.GetService<IVsEditorAdaptersFactoryService>();
            IEditorOperationsFactoryService factory = model.GetService<IEditorOperationsFactoryService>();

            EditFilter editFilter = _filter = new EditFilter(textView, factory.GetEditorOperations(textView));
            IntellisenseController intellisenseController = IntellisenseControllerProvider.GetOrCreateController(model, textView);
            var adapter = adaptersFactory.GetViewAdapter(textView);
            editFilter.AttachKeyboardFilter(adapter);
            intellisenseController.AttachKeyboardFilter();

#if DEV11
            var viewFilter = new TextViewFilter();
            viewFilter.AttachFilter(adapter);
#endif
#endif
        }

        public static void OnIdle(IOleComponentManager compMgr) {
            foreach (var window in _windows) {
                if (compMgr.FContinueIdle() == 0) {
                    break;
                }
#if FALSE
                window.Value._filter.DoIdle(compMgr);
#endif
            }
        }

        #region IVsCodeWindowManager Members

        public int AddAdornments() {
            _windows[_textView] = this;

            IVsTextView textView;

            if (ErrorHandler.Succeeded(_window.GetPrimaryView(out textView))) {
                OnNewView(textView);
            }

            if (ErrorHandler.Succeeded(_window.GetSecondaryView(out textView))) {
                OnNewView(textView);
            }
#if FALSE
            if (PythonToolsPackage.Instance.LangPrefs.NavigationBar) {
                return AddDropDownBar();
            }
#endif

            return VSConstants.S_OK;
        }
#if FALSE
        private int AddDropDownBar() {
            var pythonProjectEntry = _textView.TextBuffer.GetAnalysis() as IPythonProjectEntry;
            if (pythonProjectEntry == null) {
                return VSConstants.E_FAIL;
            }

            DropDownBarClient dropDown = _client = new DropDownBarClient(_textView, pythonProjectEntry);

            IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;

            IVsDropdownBar dropDownBar;
            int hr = manager.GetDropdownBar(out dropDownBar);
            if (ErrorHandler.Succeeded(hr) && dropDownBar != null) {
                hr = manager.RemoveDropdownBar();
                if (!ErrorHandler.Succeeded(hr)) {
                    return hr;
                }
            }

            int res = manager.AddDropdownBar(2, dropDown);
            if (ErrorHandler.Succeeded(res)) {
                _textView.TextBuffer.Properties[typeof(DropDownBarClient)] = dropDown;
            }
            return res;
        }

        private int RemoveDropDownBar() {
            if (_client != null) {
                IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;
                _client.Unregister();
                _client = null;
                _textView.TextBuffer.Properties.RemoveProperty(typeof(DropDownBarClient));
                return manager.RemoveDropdownBar();
            }
            return VSConstants.S_OK;
        }
#endif
        public int OnNewView(IVsTextView pView) {
            // TODO: We pass _textView which may not be right for split buffers, we need
            // to test the case where we split a text file and save it as an existing file?
            return VSConstants.S_OK;
        }

        public int RemoveAdornments() {
            _windows.Remove(_textView);
#if FALSE
            return RemoveDropDownBar();
#else
            return VSConstants.S_OK;
#endif
        }

        public static void ToggleNavigationBar(bool fEnable) {
#if FALSE
            foreach (var keyValue in _windows) {
                if (fEnable) {
                    ErrorHandler.ThrowOnFailure(keyValue.Value.AddDropDownBar());
                } else {
                    ErrorHandler.ThrowOnFailure(keyValue.Value.RemoveDropDownBar());
                }
            }
#endif
        }

        #endregion
    }

}