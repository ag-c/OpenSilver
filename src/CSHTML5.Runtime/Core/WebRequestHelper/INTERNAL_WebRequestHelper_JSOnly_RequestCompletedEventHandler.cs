﻿

/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>
    /// Represents the method that will handle the System.Net.WebClient.DownloadStringCompleted
    /// event of a System.Net.WebClient.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A System.Net.DownloadStringCompletedEventArgs that contains event data.</param>
    /// <returns></returns>
    internal delegate void INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventHandler(System.Object sender, INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventArgs e);
}