using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;

namespace DZModForger
{
    public sealed partial class ModelHierarchyPanel : UserControl
    {
        public ModelHierarchyViewModel ViewModel { get; private set; }

        public ModelHierarchyPanel()
        {
            this.InitializeComponent();
            ViewModel = new ModelHierarchyViewModel();
            this.DataContext = ViewModel;
        }

        private void TreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem is ModelNode selectedNode)
            {
                ViewModel.OnNodeSelected(selectedNode);
            }
        }
    }

    public class ModelNode
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsExpanded { get; set; }
        public ObservableCollection<ModelNode> Children { get; set; }

        public ModelNode()
        {
            Children = new ObservableCollection<ModelNode>();
            IsExpanded = true;
        }
    }
}
