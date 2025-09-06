using System;
using System.Windows.Data;
using System.Windows.Markup;
namespace Termino.Localization.Wpf;
[MarkupExtensionReturnType(typeof(BindingExpression))]
public class LocExtension:MarkupExtension{
    public string Key{get;set;}
    public LocExtension(string key)=>Key=key;
    public override object ProvideValue(IServiceProvider sp)=> new Binding($"[{Key}]"){ Source=LocalizerProxy.Instance, Mode=BindingMode.OneWay }.ProvideValue(sp);
}