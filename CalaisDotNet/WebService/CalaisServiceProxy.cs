﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

// 
// This source code was auto-generated by wsdl, Version=2.0.50727.1432.
// 


/// <remarks/>
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.1432")]
public partial class CalaisServiceProxy : System.Web.Services.Protocols.HttpPostClientProtocol {
    
    private System.Threading.SendOrPostCallback EnlightenOperationCompleted;
    
    /// <remarks/>
    public CalaisServiceProxy() {
        this.Url = "http://api.opencalais.com/enlighten/calais.asmx";
    }
    
    /// <remarks/>
    public event EnlightenCompletedEventHandler EnlightenCompleted;
    
    /// <remarks/>
    [System.Web.Services.Protocols.HttpMethodAttribute(typeof(System.Web.Services.Protocols.XmlReturnReader), typeof(System.Web.Services.Protocols.HtmlFormParameterWriter))]
    [return: System.Xml.Serialization.XmlRootAttribute("string", Namespace="http://clearforest.com/", IsNullable=true)]
    public string Enlighten(string licenseID, string content, string paramsXML) {
        return ((string)(this.Invoke("Enlighten", (this.Url + "/Enlighten"), new object[] {
                    licenseID,
                    content,
                    paramsXML})));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginEnlighten(string licenseID, string content, string paramsXML, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Enlighten", (this.Url + "/Enlighten"), new object[] {
                    licenseID,
                    content,
                    paramsXML}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndEnlighten(System.IAsyncResult asyncResult) {
        return ((string)(this.EndInvoke(asyncResult)));
    }
    
    /// <remarks/>
    public void EnlightenAsync(string licenseID, string content, string paramsXML) {
        this.EnlightenAsync(licenseID, content, paramsXML, null);
    }
    
    /// <remarks/>
    public void EnlightenAsync(string licenseID, string content, string paramsXML, object userState) {
        if ((this.EnlightenOperationCompleted == null)) {
            this.EnlightenOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEnlightenOperationCompleted);
        }
        this.InvokeAsync("Enlighten", (this.Url + "/Enlighten"), new object[] {
                    licenseID,
                    content,
                    paramsXML}, this.EnlightenOperationCompleted, userState);
    }
    
    private void OnEnlightenOperationCompleted(object arg) {
        if ((this.EnlightenCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.EnlightenCompleted(this, new EnlightenCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    public new void CancelAsync(object userState) {
        base.CancelAsync(userState);
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.1432")]
public delegate void EnlightenCompletedEventHandler(object sender, EnlightenCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.1432")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class EnlightenCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal EnlightenCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public string Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((string)(this.results[0]));
        }
    }
}