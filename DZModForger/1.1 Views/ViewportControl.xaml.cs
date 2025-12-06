using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Security.Claims;
using System.Xml.Linq;

<? xml version = "1.0" encoding = "utf-8" ?>
< UserControl
    x:Class = "DZModForger.ViewportControl"
    xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns: x = "http://schemas.microsoft.com/winfx/2006/xaml" >

    < Grid Background = "#1a1a1a" >
        < SwapChainPanel
            x: Name = "DxSwapChainPanel"
            PointerPressed = "SwapChainPanel_PointerPressed"
            PointerMoved = "SwapChainPanel_PointerMoved"
            PointerReleased = "SwapChainPanel_PointerReleased"
            PointerWheelChanged = "SwapChainPanel_PointerWheelChanged"
            PointerEntered = "SwapChainPanel_PointerEntered"
            PointerExited = "SwapChainPanel_PointerExited"
            ManipulationMode = "All" />
    </ Grid >
</ UserControl >
