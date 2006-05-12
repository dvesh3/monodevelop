//
// WindowsFolderNodeBuilder.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.GtkCore.GuiBuilder;

namespace MonoDevelop.GtkCore.NodeBuilders
{
	public class WindowsFolderNodeBuilder: TypeNodeBuilder
	{
		WindowEventHandler updateDelegate;
		Stetic.Wrapper.ActionGroupEventHandler updateDelegate2;
		
		public WindowsFolderNodeBuilder ()
		{
			updateDelegate = (WindowEventHandler) MonoDevelop.Core.Gui.Services.DispatchService.GuiDispatch (new WindowEventHandler (OnUpdateFiles));
			updateDelegate2 = (Stetic.Wrapper.ActionGroupEventHandler) MonoDevelop.Core.Gui.Services.DispatchService.GuiDispatch (new Stetic.Wrapper.ActionGroupEventHandler (OnUpdateFiles));
		}
		
		public override Type NodeDataType {
			get { return typeof(WindowsFolder); }
		}
		
		public override Type CommandHandlerType {
			get { return typeof(UserInterfaceCommandHandler); }
		}
		
		public override string ContextMenuAddinPath {
			get { return "/SharpDevelop/Views/ProjectBrowser/ContextMenu/WidgetsNode"; }
		}
		
		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			return "UserInterface";
		}
		
		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Gdk.Pixbuf icon, ref Gdk.Pixbuf closedIcon)
		{
			label = GettextCatalog.GetString ("User Interface");
			icon = Context.GetIcon (Stock.OpenResourceFolder);
			closedIcon = Context.GetIcon (Stock.ClosedResourceFolder);
		}

		public override void BuildChildNodes (ITreeBuilder builder, object dataObject)
		{
			Project p = ((WindowsFolder)dataObject).Project;
			GtkDesignInfo info = GtkCoreService.GetGtkInfo (p);
			if (info != null) {
				foreach (GuiBuilderWindow fi in info.GuiBuilderProject.Windows)
					builder.AddChild (fi);
				foreach (Stetic.Wrapper.ActionGroup group in info.GuiBuilderProject.SteticProject.ActionGroups)
					builder.AddChild (group);
			}
		}

		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			Project p = ((WindowsFolder)dataObject).Project;
			GtkDesignInfo info = GtkCoreService.GetGtkInfo (p);
			return (info != null && !info.GuiBuilderProject.IsEmpty);
		}
		
		public override int CompareObjects (ITreeNavigator thisNode, ITreeNavigator otherNode)
		{
			if (otherNode.DataItem is ResourceFolder || otherNode.DataItem is ProjectReferenceCollection)
				return 1;
			else
				return -1;
		}

		public override void OnNodeAdded (object dataObject)
		{
			Project p = ((WindowsFolder)dataObject).Project;
			GtkDesignInfo info = GtkCoreService.GetGtkInfo (p);
			info.GuiBuilderProject.WindowAdded += updateDelegate;
			info.GuiBuilderProject.WindowRemoved += updateDelegate;
			info.GuiBuilderProject.SteticProject.ActionGroups.ActionGroupAdded += updateDelegate2;
			info.GuiBuilderProject.SteticProject.ActionGroups.ActionGroupRemoved += updateDelegate2;
		}
		
		public override void OnNodeRemoved (object dataObject)
		{
			Project p = ((WindowsFolder)dataObject).Project;
			GtkDesignInfo info = GtkCoreService.GetGtkInfo (p);
			info.GuiBuilderProject.WindowAdded -= updateDelegate;
			info.GuiBuilderProject.WindowRemoved -= updateDelegate;
			info.GuiBuilderProject.SteticProject.ActionGroups.ActionGroupAdded -= updateDelegate2;
			info.GuiBuilderProject.SteticProject.ActionGroups.ActionGroupRemoved -= updateDelegate2;
		}
		
		void OnUpdateFiles (object s, WindowEventArgs args)
		{
			ITreeBuilder tb = Context.GetTreeBuilder (args.Window.Project.Project);
			if (tb != null) {
				if (tb.MoveToChild ("UserInterface", typeof(WindowsFolder))) {
					tb.UpdateAll ();
				}
			}
		}
		
		void OnUpdateFiles (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			ITreeBuilder tb = Context.GetTreeBuilder (args.ActionGroup);
			if (tb != null) {
				tb.MoveToParent ();
				tb.UpdateAll ();
			}
		}
	}
}
