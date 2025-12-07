using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

[ComImport]
[Guid("94D99BDB-F1F8-4AB0-B236-7EA0B6B5A1A5")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface ISwapChainPanelNative
{
    int SetSwapChain(IntPtr swapChain);
}

namespace DZModForger
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            InitializeDXInterop();
        }

        private void InitializeDXInterop()
        {
            IntPtr swapChainPanel = IntPtr.Zero;
            if (swapChainPanel != IntPtr.Zero)
            {
                try
                {
                    ISwapChainPanelNative nativePanel = this.swapChainPanel.As<ISwapChainPanelNative>();

                    int hr = nativePanel.SetSwapChain(swapChainPanel);

                    if (hr != 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"SetSwapChain failed HRESULT: {hr}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error accessing native interface: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DirectX SwapChain pointer is null. Cannot initialize SwapChainPanel.");
            }
        }
    }
}