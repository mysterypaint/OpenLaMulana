using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace MidiLooper;
class Program
{
    [DllImport("csvmidi.so", EntryPoint = "csvmidi")]
    static extern int csvmidi(int argc, string[] argv);

    [DllImport("midicsv.so", EntryPoint = "midicsv")]
    static extern int midicsv(int argc, string[] argv);

    static void Main(string[] args)
    {
        string path = ".";

        double[] quarterBeatToLoopAt = new double[] {
            16,	//	m00
	        16,	//	m01
	        8,	//	m02
	        (16*4)+1,	//	m03
	        16,	//	m04
	        8,	//	m05
	        4,	//	m06
	        8,	//	m07
	        8,	//	m08
	        8,	//	m09
	        (8*4)+1,	//	m10
	        13.5,	//	m11
	        16,	//	m12
	        8,	//	m13
	        8,	//	m14
	        28,	//	m15
	        16,	//	m16
	        32,	//	m17
	        0,	//	m18
	        -1,	//	m19
	        0,	//	m20
	        8*4,	//	m21
	        -1,	//	m22
	        8,	//	m23
	        10*4,	//	m24
	        4*4,	//	m25
	        4*4,	//	m26
	        4*4,	//	m27
	        4*8,	//	m28
	        4*6,	//	m29
	        16,	//	m30
	        16,	//	m31
	        4*2,	//	m32
	        16,	//	m33
	        0,	//	m34
	        0,	//	m35
	        2,	//	m36
	        -1,	//	m37
	        6,	//	m38
	        10*4,	//	m39
	        8*4,	//	m40
	        0,	//	m41
	        (7*4)+2,	//	m42
	        16,	//	m43
	        16,	//	m44
	        4,	//	m45
	        0,	//	m46
	        (24*4)+1,	//	m47
	        0,	//	m48
	        16,	//	m49
	        8*4,	//	m50
	        4,	//	m51
	        8,	//	m52
	        4*3,	//	m53
	        -1,	//	m54
	        0,	//	m55
	        -1,	//	m56
	        0,	//	m57
	        16,	//	m58
	        2,	//	m59
	        3*9,	//	m60
	        16,	//	m61
	        6*4,	//	m62
	        19,	//	m63
	        16,	//	m64
	        5*4+1,	//	m65
	        8*4,	//	m66
	        16,	//	m67
	        0,	//	m68
	        1,	//	m69
	        0,	//	m70
	        0,	//	m71
	        4,	//	m72
	        16,	//	m73
	        (16*4)+1,	//	m74
	        0,	//	m75
        };

        int[] loopExceptionTracks = { 10, 7, 11, 19, 21, 22, 28, 30, 35, 36, 37, 40, 41, 42, 48, 54, 56, 57, 59, 60, 61, 63, 70, 71, 74};
        /*
        for (int i = 0; i < 75; i++) {
            string preInd = "";
            if (i < 10)
                preInd += "0";
            string fNm = "m" + preInd + i;
            Debug.WriteLine(String.Format("{0}, // {1}", 16, fNm));
        }*/

        string inDir = Path.Combine(path, "input");
        string outDir = Path.Combine(path, "output");

        System.IO.Directory.CreateDirectory(inDir);
        System.IO.Directory.CreateDirectory(outDir);
        System.IO.Directory.CreateDirectory(Path.Combine(outDir, "csv"));

        string[] files = System.IO.Directory.GetFiles(inDir, "*.mid");

        foreach (string f in files)
        {
            string fName = Path.GetFileNameWithoutExtension(f);
            string sIn = Path.Combine(inDir, fName + ".mid");
            string sOut = Path.Combine(outDir, "csv", fName + ".csv");

            int index = Int32.Parse(fName.Substring(1, fName.Length - 1));

            ProcessMidiToCSV(sIn, sOut);

            bool loopAtEnd = false;

            for (var i = 0; i < loopExceptionTracks.Length; i++) {
                if (loopExceptionTracks[i] == index)
                    loopAtEnd = true;
            }
            ModifyCSV(quarterBeatToLoopAt[index], sOut, loopAtEnd, index);

            sIn = Path.Combine(outDir, "csv", fName + ".csv");
            sOut = Path.Combine(outDir, fName + ".mid");

            ProcessCSVToMidi(sIn, sOut);
        }

    }

    private static void ProcessCSVToMidi(string sIn, string sOut)
    {
        Process csvmidi = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "csvmidi",
                Arguments = sIn + " " + sOut,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        csvmidi.Start();
        while (!csvmidi.StandardOutput.EndOfStream)
        {
        }
    }

    private static void ModifyCSV(double quarterBeatToLoopAt, string sOut, bool loopAtEnd, int songIndex)
    {
        // Open the file to read from.
        int counter = 0;
        int ticksPerQuarter = 1;
        int loopTick = 0;
        List<string> contents = new List<string>();
        List<int> trackLengths = new List<int>();
        List<int> trackEndLines = new List<int>();

        int globalEndTrackLine = 0;
        int previousEventTick = 0;
        int longestChannel = 0;

        // Read the file and display it line by line.
        int currChannel = 0;
        bool loopPlaced = false;
        bool firstNoteOnForChannel = false;

        foreach (string line in System.IO.File.ReadLines(sOut))
        {
            string[] lineArgs = line.Split(", ");
            string cmd = lineArgs[2];
            int value = Int32.Parse(lineArgs[1]);

            if (currChannel == 1) {
                if (value > loopTick && !loopPlaced && quarterBeatToLoopAt >= 0) {
                    contents.Add("1, " + loopTick + ", Marker_t, \"Loop\"");
                    counter++;
                    loopPlaced = true;
                }
            }
            
            switch (cmd.ToLower())
            {
                case "note_on_c":
                    if (!firstNoteOnForChannel) {
                        if (songIndex == 28)
                        {
                            switch (currChannel)
                            {
                                case 2:
                                    contents.Add("2, 0, Program_c, 0, 39");
                                    firstNoteOnForChannel = true;
                                    break;
                                case 5:
                                    contents.Add("5, 0, Program_c, 3, 39");
                                    firstNoteOnForChannel = true;
                                    break;
                                case 7:
                                    contents.Add("7, 0, Program_c, 5, 39");
                                    firstNoteOnForChannel = true;
                                    break;
                            }
                        }
                    }
                    break;
                case "header":
                    ticksPerQuarter = Int32.Parse(lineArgs[5]);
                    loopTick = (int)Math.Round(quarterBeatToLoopAt * ticksPerQuarter);
                    /*
                    switch (songIndex) {
                        case 42:
                            loopTick += (int)(quarterBeatToLoopAt % (ticksPerQuarter * 4));
                            break;
                    }*/
                    currChannel++;
                    firstNoteOnForChannel = false;
                    break;
                case "end_track":
                    if (lineArgs[0].Equals("1"))
                    {
                        if (!loopPlaced) {
                            if (quarterBeatToLoopAt >= 0)
                                contents.Add("1, " + loopTick + ", Marker_t, \"Loop\"");
                            counter++;
                            loopPlaced = true;
                        }

                        globalEndTrackLine = counter;
                        trackEndLines.Add(globalEndTrackLine);
                    }
                    else
                    {
                        int trackLen = previousEventTick;// Int32.Parse(lineArgs[1]);
                        trackLengths.Add(trackLen);
                        trackEndLines.Add(counter + 1);

                        if (longestChannel < trackLen)
                            longestChannel = trackLen;
                    }
                    currChannel++;
                    firstNoteOnForChannel = false;
                    break;
            }

            contents.Add(line);
            counter++;

            previousEventTick = Int32.Parse(lineArgs[1]);
        }

        int[] arr = trackLengths.ToArray();
        int lastEventTick = 0;

        if (loopAtEnd)
            lastEventTick = longestChannel;
        else
            lastEventTick = MostFrequent(arr);

        if (songIndex == 11 || songIndex == 60)
            ticksPerQuarter /= 2;
        else if (songIndex == 14)
            lastEventTick += (ticksPerQuarter * 4 * 3);
        else if (songIndex == 63)
            lastEventTick = (ticksPerQuarter * ((36 * 4) + 3));

        int closestBeatInTicks = (int)(Math.Round((double)lastEventTick / ticksPerQuarter) * ticksPerQuarter);

        if (songIndex == 59)
            closestBeatInTicks += (ticksPerQuarter / 2);

        string endOfTrack1 = contents[globalEndTrackLine];
        string[] endTrackArgs = endOfTrack1.Split(", ");
        endTrackArgs[1] = closestBeatInTicks.ToString(); // Bump the loop end to the next-closest quarter beat, just in case
        contents[globalEndTrackLine] = String.Join(", ", endTrackArgs);

        // int removedElements = 0;

        /* This optimization is very buggy...

        for (int i = 0; i < trackEndLines.Count; i++)
        {
            string endOfChTrack = contents[trackEndLines[i] - removedElements];
            string[] chEndTrackArgs = endOfChTrack.Split(", ");
            chEndTrackArgs[1] = closestBeatInTicks.ToString(); // Bump the loop end to the next-closest quarter beat, just in case
            contents[trackEndLines[i] - removedElements] = String.Join(", ", chEndTrackArgs);


            // Get rid of any events longer than this channel)
            int lastLineInChannel = trackEndLines[i] - removedElements;
            int checkingLine = lastLineInChannel - 1;
            string huh = contents[checkingLine].Split(", ")[1];
            while (Int32.Parse(huh) >= closestBeatInTicks) {
                if(contents[checkingLine].Split(", ")[2].Equals("End_track"))
                    break;
                contents.RemoveAt(checkingLine);
                removedElements++;

                huh = contents[checkingLine].Split(", ")[1];
            }
        } */

        // Inject a Loop End marker
        contents.Insert(globalEndTrackLine, "1, " + closestBeatInTicks + ", Marker_t, \"LoopEnd\"");

        // Create a file to write to.
        StreamWriter file = new System.IO.StreamWriter(sOut);
        contents.ForEach(file.WriteLine);
        file.Close();


    }

    private static void ProcessMidiToCSV(string sIn, string sOut)
    {
        //midicsv(2, new string[] { sIn, sOut});
        Process midicsv = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "midicsv",
                Arguments = sIn + " " + sOut,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        midicsv.Start();
        while (!midicsv.StandardOutput.EndOfStream)
        {
        }
    }

    static int MostFrequent(int[] arr)
    {
        int maxCount = 0;
        int mostFreqElement = arr[0];
        for (int i = 0; i < arr.Length; i++)
        {
            int count = 0;
            for (int j = 0; j < arr.Length; j++)
            {
                if (arr[i] == arr[j])
                    count++;
            }

            if (count > maxCount)
            {
                maxCount = count;
                mostFreqElement = arr[i];
            }
        }

        return mostFreqElement;
    }
}
