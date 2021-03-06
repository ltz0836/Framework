﻿using System;
using Framework.UI.Wrap.Base;
using UnityEngine.Events;

namespace Framework.UI.Core.Bind
{
    public class BindField<TComponent, TData>
    {
        private TComponent _component;
        private Action<TData> _propChangeCb;
        private UnityEvent<TData> _componentEvent;
        private Func<TData, TData> _prop2CpntWrap;
        private Func<TData, TData> _cpnt2PropWrap;
        private ObservableProperty<TData> _property;
        private object _defaultWrapper;
        private BindType _bindType;

        public BindField(TComponent component, ObservableProperty<TData> property, Action<TData> fieldChangeCb,
            UnityEvent<TData> componentEvent, BindType bindType,
            Func<TData, TData> property2CpntWrap, Func<TData, TData> cpnt2PropWrap)
        {
            SetValue(component, property, fieldChangeCb, componentEvent, bindType, property2CpntWrap,
                cpnt2PropWrap);
            InitEvent();
            InitCpntValue();
        }

        public void UpdateValue(TComponent component, ObservableProperty<TData> property, Action<TData> fieldChangeCb,
            UnityEvent<TData> componentEvent, BindType bindType,
            Func<TData, TData> property2CpntWrap, Func<TData, TData> cpnt2PropWrap)
        {
            SetValue(component, property, fieldChangeCb, componentEvent, bindType, property2CpntWrap,
                cpnt2PropWrap);
            InitCpntValue();
        }

        private void SetValue(TComponent component, ObservableProperty<TData> property, Action<TData> fieldChangeCb,
            UnityEvent<TData> componentEvent, BindType bindType,
            Func<TData, TData> property2CpntWrap, Func<TData, TData> cpnt2PropWrap)
        {
            this._component = component;
            this._property = property;
            this._bindType = bindType;
            _prop2CpntWrap = property2CpntWrap;
            this._cpnt2PropWrap = cpnt2PropWrap;
            _propChangeCb = fieldChangeCb;
            this._componentEvent = componentEvent;
        }

        /// <summary>
        /// 将field的值初始化给component显示
        /// </summary>
        private void InitCpntValue()
        {
            if (_bindType != BindType.OnWay) return;
            _propChangeCb(_prop2CpntWrap == null ? _property.Value : _prop2CpntWrap(_property.Value));
        }

        private void InitEvent()
        {
            _defaultWrapper = BindTool.GetDefaultWrapper(_component);
            if (_propChangeCb == null)
                _propChangeCb = (_defaultWrapper as IFieldChangeCb<TData>)?.GetFieldChangeCb();
            if (_componentEvent == null)
                _componentEvent = (_defaultWrapper as IComponentEvent<TData>)?.GetComponentEvent();
            switch (_bindType)
            {
                case BindType.OnWay:
                    Log.Assert(_propChangeCb != null);
                    if(_propChangeCb == null) return;
                    _property.AddListener((value) =>
                        _propChangeCb(_prop2CpntWrap == null ? value : _prop2CpntWrap(value)));
                    break;
                case BindType.Revert:
                    Log.Assert(_componentEvent != null);
                    if(_componentEvent == null) return;
                    _componentEvent.AddListener((data) =>
                        _property.Value = _cpnt2PropWrap == null ? data : _cpnt2PropWrap(data));
                    break;
            }
        }
    }

    public class BindField<TComponent, TData1, TData2, TResult> where TComponent : class
    {
        private TComponent _component;
        private Action<TResult> _filedChangeCb;
        private ObservableProperty<TData1> _property1;
        private ObservableProperty<TData2> _property2;
        private Func<TData1, TData2, TResult> _wrapFunc;
        private object _defaultWrapper;

        public BindField(TComponent component, ObservableProperty<TData1> property1, ObservableProperty<TData2> property2,
            Func<TData1, TData2, TResult> wrapFunc, Action<TResult> filedChangeCb = null)
        {
            SetValue(component, property1, property2, wrapFunc, filedChangeCb);
            InitEvent();
            InitCpntValue();
        }

        public void UpdateValue(TComponent component, ObservableProperty<TData1> property1,
            ObservableProperty<TData2> property2,
            Func<TData1, TData2, TResult> wrapFunc, Action<TResult> filedChangeCb)
        {
            SetValue(component, property1, property2, wrapFunc, filedChangeCb);
            InitCpntValue();
        }

        private void SetValue(TComponent component, ObservableProperty<TData1> property1,
            ObservableProperty<TData2> property2,
            Func<TData1, TData2, TResult> wrapFunc, Action<TResult> filedChangeCb)
        {
            this._component = component;
            this._property1 = property1;
            this._property2 = property2;
            this._wrapFunc = wrapFunc;
            this._filedChangeCb = filedChangeCb;
        }

        private void InitCpntValue()
        {
            _filedChangeCb(_wrapFunc(_property1.Value, _property2.Value));
        }

        private void InitEvent()
        {
            _defaultWrapper = BindTool.GetDefaultWrapper(_component);
            _filedChangeCb = _filedChangeCb ?? (_defaultWrapper as IFieldChangeCb<TResult>)?.GetFieldChangeCb();
            _property1.AddListener((data1) => _filedChangeCb(_wrapFunc(data1, _property2.Value)));
            _property2.AddListener((data2) => _filedChangeCb(_wrapFunc(_property1.Value, data2)));
            _filedChangeCb(_wrapFunc(_property1.Value, _property2.Value));
        }
    }
}