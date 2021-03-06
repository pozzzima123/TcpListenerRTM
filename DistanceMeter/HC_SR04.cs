﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace DistanceMeter
{
    class HC_SR04
    {
        private GpioController _controller;
        private GpioPin _triggerPin;
        private GpioPin _echoPin;
        public GpioPin LedPin1;
        public GpioPin LedPin2;

        private readonly int ECHO_PIN;
        private readonly int TRIGGER_PIN;
        private readonly int LED_PIN1;
        private readonly int LED_PIN2;

        private readonly Stopwatch _measureEcho = new Stopwatch();
        private readonly Stopwatch _repeater = new Stopwatch();

        private readonly List<long> _measure = new List<long>();
        private bool _ready = false;

        public int CountOfMeasure = 10;



        public HC_SR04(int ECHO_PIN, int TRIGGER_PIN, int LED_PIN1, int LED_PIN2)
        {
            this.ECHO_PIN = ECHO_PIN;
            this.TRIGGER_PIN = TRIGGER_PIN;
            this.LED_PIN1 = LED_PIN1;
            this.LED_PIN2 = LED_PIN2;
        }

        public void Init()
        {
            _controller = GpioController.GetDefault();
            if (_controller != null)
            {
                Debug.WriteLine("Git");
            }
            _triggerPin = _controller?.OpenPin(TRIGGER_PIN);
            _triggerPin.SetDriveMode(GpioPinDriveMode.Output);
            _triggerPin.Write(GpioPinValue.Low);

            LedPin1 = _controller?.OpenPin(LED_PIN1);
            LedPin1.SetDriveMode(GpioPinDriveMode.Output);
            LedPin1.Write(GpioPinValue.Low);

            LedPin2 = _controller?.OpenPin(LED_PIN2);
            LedPin2.SetDriveMode(GpioPinDriveMode.Output);
            LedPin2.Write(GpioPinValue.Low);

            _echoPin = _controller?.OpenPin(ECHO_PIN);
            _echoPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            Task.Delay(60).Wait();
            _echoPin.ValueChanged += Detect;
        }

        private void Detect(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            //wysylasz tylko jedno polecenie pomiaru
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                //zatrzymaj timer i oblicz na jego podstawie odleglosc
                _measureEcho.Stop();
                long measured = _measureEcho.ElapsedTicks/(Stopwatch.Frequency/(1000L*1000L))/105;
                //jesli pomiar nie miesci sie w przedziale , jest pewność , że nie jest on prawidlowy
                //czekaj na kolejny pomiar
                if (measured < 2 || measured > 400)
                {
                    SendTrigger();
                }
                else
                {
                    //dodaj pomiar do listy
                    _measure.Add(measured);

                    //czekaj 60ms na nastepny pomiar- WAŻNE
                    _echoPin.ValueChanged -= Detect;
                    Task.Delay(60).Wait();
                    _echoPin.ValueChanged += Detect;
                    _ready = true;
                }
            }
        }

        private void SendTrigger()
        {
            //wyslij komende do odczytu
            _triggerPin.Write(GpioPinValue.High);
            WaitUs(10);
            _triggerPin.Write(GpioPinValue.Low);
            //zacznij odliczanie po ktorym nastepuje wywolanie eventu Detect
            _measureEcho.Restart();
        }

        public double Distance
        {
            // ReSharper disable once FunctionRecursiveOnAllPaths
            get
            {
                //zwroc srednia arytmetyczna pomiarów
                for (int i = 0; i < CountOfMeasure; i++)
                {
                    _ready = false;
                    SendTrigger();
                    while (_ready != true)
                    {
                    }
                }
                double toGet = _measure.Average();
                _measure.Clear();
                return toGet;
            }
        }

        private void WaitUs(int us)
        {
            //timer wysokoprecyzyjny
            _repeater.Restart();
            while (_repeater.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L)) <= us)
            {

            }
            _repeater.Stop();
        }

    }
}
