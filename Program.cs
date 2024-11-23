// See https://aka.ms/new-console-template for more information

// Create a new MIDI input device

using System.Diagnostics;
using Sanford.Multimedia.Midi;

SynchronizationContext context;
InputDevice midiIn;

Stopwatch fWatch = Stopwatch.StartNew();
double fLastMillis = 0d;
double fDiff = 0d;
double fDiffTimeStamp = 0d;
int counter = 0;
int fLastTimestamp = 0;

try
{
    context = SynchronizationContext.Current;

    midiIn = new InputDevice(0);
    midiIn.MessageReceived += MidiInOnMessageReceived;
    midiIn.ChannelMessageReceived += HandleChannelMessageReceived;
    midiIn.SysCommonMessageReceived += HandleSysCommonMessageReceived;
    midiIn.SysExMessageReceived += HandleSysExMessageReceived;
    midiIn.SysRealtimeMessageReceived += HandleSysRealtimeMessageReceived;
    midiIn.Error += HandleError;
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

Console.WriteLine("Reading MIDI device inputs...");
Console.WriteLine("Press Q to quit.");
while (true)
{
    if (InputDevice.DeviceCount == 0)
    {
        Console.WriteLine("No MIDI input devices available.");
        break;
    }

    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
    {
        midiIn.Close();
        break;
    }
}

return;

void MidiInOnMessageReceived(IMidiMessage message)
{
    context.Post(_ => Console.WriteLine(message.ToString()), null);
}

void HandleError(object? sender, Sanford.Multimedia.ErrorEventArgs e)
{
    context.Post(_ => Console.WriteLine(e.ToString()), null);
}

void HandleChannelMessageReceived(object? sender, ChannelMessageEventArgs e)
{
    context.Post(_ => Console.WriteLine(e.Message.Command.ToString() + '\t' + '\t' +
                                        e.Message.MidiChannel.ToString() + '\t' +
                                        e.Message.Data1.ToString() + '\t' +
                                        e.Message.Data2), null);
}

void HandleSysExMessageReceived(object? sender, SysExMessageEventArgs e)
{
}

void HandleSysCommonMessageReceived(object? sender, SysCommonMessageEventArgs e)
{
    context.Post(_ => Console.WriteLine(e.Message.SysCommonType.ToString() + '\t' + '\t' +
                                        e.Message.Data1.ToString() + '\t' +
                                        e.Message.Data2.ToString()), null);
}

void HandleSysRealtimeMessageReceived(object sender, SysRealtimeMessageEventArgs e)
{
    counter++;
    if (counter % 24 == 0)
    {
        var millis = fWatch.Elapsed.TotalMilliseconds;
        fDiff = 60000 / (millis - fLastMillis);
        fLastMillis = millis;

        var timestamp = e.Message.Timestamp;
        fDiffTimeStamp = 60000.0 / (timestamp - fLastTimestamp);
        fLastTimestamp = timestamp;
    }

    context.Post(_ =>
    {
        Console.WriteLine(e.Message.SysRealtimeType.ToString());

        Console.WriteLine("BPM from stopwatch: " + fDiff.ToString("F4"));
        Console.WriteLine("BPM from driver timestamp: " + fDiffTimeStamp.ToString("F4"));
    }, null);
}
