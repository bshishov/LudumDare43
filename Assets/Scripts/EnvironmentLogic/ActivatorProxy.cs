using System;
using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class ActivatorProxy : MonoBehaviour
    {
        public static string ActivateEvent = "OnActivate";
        public static string DeActivateEvent = "OnDeactivate";
        public static string ProxyActivateEvent = "OnProxyActivate";
        public static string ProxyDeActivateEvent = "OnProxyDeactivate";

        public bool IsActivated { get; private set; }

        public bool HoldActivation = true;
        public bool Inverted = false;
        public float TimeBeforeActivationMessage;
        public float TimeBeforeDeactivationMessage;
        public event Action Activated;
        public event Action Deactivated;

        private int _counter = 0;
        private float _deactivatingTime;
        private bool _deactivationPending;
        private bool _activationPending;
        private float _activationTime;

        void Start()
        {
            if (Inverted)
            {
                ActualDeactivate();
            }
        }

        void Update()
        {
            if (_deactivationPending && Time.time > _deactivatingTime)
            {
                ActualDeactivate();
                _deactivationPending = false;
            }

            if (_activationPending && Time.time > _activationTime)
            {
                ActualActivate();
                _activationPending = false;
            }
        }

        // Coming from SendMessage
        void OnActivate()
        {
            if (HoldActivation)
            {
                _counter++;

                if (_deactivationPending)
                    _deactivationPending = false;

                if (_counter == 1)
                {
                    if (!_activationPending)
                    {
                        _activationPending = true;
                        _activationTime = Time.time + TimeBeforeActivationMessage;
                    }
                }
            }
            else if (!_activationPending && !_deactivationPending)
            {
                _activationPending = true;
                _activationTime = Time.time + TimeBeforeActivationMessage;
                _deactivationPending = true;
                _deactivatingTime = Mathf.Max(_activationTime, Time.time) + TimeBeforeDeactivationMessage;
            }
        }

        // Coming from SendMessage
        void OnDeactivate()
        {
            if (HoldActivation)
            {
                _counter--;
                if (_counter < 0)
                    _counter = 0;

                if (_counter == 0)
                {
                    _deactivationPending = true;
                    _deactivatingTime = Mathf.Max(_activationTime, Time.time) + TimeBeforeDeactivationMessage;
                }
            }
        }

        public void Activate()
        {
            OnActivate();
        }

        public void Deactivate()
        {
            OnDeactivate();
        }

        void ActualActivate()
        {
            if (!Inverted)
            {
                IsActivated = true;
                gameObject.SendMessage(ProxyActivateEvent, SendMessageOptions.DontRequireReceiver);
                if(Activated != null)
                    Activated.Invoke();
            }
            else
            {
                IsActivated = false;
                gameObject.SendMessage(ProxyDeActivateEvent, SendMessageOptions.DontRequireReceiver);
                if (Deactivated != null)
                    Deactivated.Invoke();
            }
        }
    
        void ActualDeactivate()
        {
            if (!Inverted)
            {
                IsActivated = false;
                gameObject.SendMessage(ProxyDeActivateEvent, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                IsActivated = true;
                gameObject.SendMessage(ProxyActivateEvent, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
