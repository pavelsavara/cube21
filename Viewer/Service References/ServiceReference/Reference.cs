﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1378
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Viewer.ServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.ICube21Service")]
    public interface ICube21Service {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ICube21Service/FindWayHome", ReplyAction="http://tempuri.org/ICube21Service/FindWayHomeResponse")]
        Zamboch.Cube21.Actions.SmartStep[] FindWayHome(Zamboch.Cube21.Cube cube);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface ICube21ServiceChannel : Viewer.ServiceReference.ICube21Service, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class Cube21ServiceClient : System.ServiceModel.ClientBase<Viewer.ServiceReference.ICube21Service>, Viewer.ServiceReference.ICube21Service {
        
        public Cube21ServiceClient() {
        }
        
        public Cube21ServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public Cube21ServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Cube21ServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Cube21ServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Zamboch.Cube21.Actions.SmartStep[] FindWayHome(Zamboch.Cube21.Cube cube) {
            return base.Channel.FindWayHome(cube);
        }
    }
}
