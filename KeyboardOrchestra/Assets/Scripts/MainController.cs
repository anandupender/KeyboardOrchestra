﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour {

	//Instruciton Text, input Text 1, input Text 2, action type
	private string[,,] specialWords = new string[,,] { 
		{ { "Welcome to the Keyboard Orchestra. Type start to begin.", "", "start", "greyOut" }, {"Welcome to the Keyboard Orchestra. Type start to begin.", "", "start", "greyOut" } },
		{ { "You are player...", "", "one", "greyOut"}, { "You are player...", "", "two", "greyOut"} },
		{ { "You can also play your partner's keyboard by pressing keys at the same time to make...", "a", "team", "greyOut"}, { "You can also play your partner's keyboard by pressing keys at the same time", "go", "team", "greyOut"} },
		{ { "Ready to get Started?", "", "ready", "greyOut"}, {"Ready to get Started?", "", "ready", "greyOut"} },
		{ { "Waiting for next instruction...", "", "", "waiting"}, { "Waiting for next instruction...", "", "", "waiting"} },
		{ { "Lay down the bass", "", "bass", "greyOut"}, {"Waiting for next instruction...", "", "", "waiting"} },
		{ { "Add the Synth Melody", "", "synth", "greyOut"}, { "Add the Synth Melody", "", "synth", "greyOut"} },
		{ { "Add a harmony...", "", "now", "greyOut"}, {"Waiting for next instruction...", "", "", "waiting"} },
		{ { "Raise the key","key","","greyOut"}, { "", "", "", ""} },
		{ { "Lower the key back down","lower","","greyOut"}, { "", "", "", ""} },
		{ { "Pause the Melody ","pause","","greyOut"}, { "", "", "", ""} },
		{ { "Add a funky voice","funky","","greyOut"}, { "", "", "", ""} }
	};

	public GameObject step1;

	ChuckInstance myChuck;
	Chuck.FloatCallback myGetPosCallback;

	private MyStepController step1Script;

	private int currRound = 0;
	private float myPos;
	private float previousPos;
	private bool updatedRound;

	// Use this for initialization
	void Start () {
		updatedRound = false;
		myPos = 0.0f;
		previousPos = 0.0f;
		step1Script = step1.GetComponent<MyStepController> ();
		myChuck = GetComponent<ChuckInstance> ();
		myGetPosCallback = Chuck.CreateGetFloatCallback( GetPosCallback );
	}
	
	// Update is called once per frame
	void Update () {
		getKey ();
		if (!updatedRound) {
			//update
			step1Script.stepInstructions = oneD(currRound,0);
			Debug.Log ("step 1 instruction set" + step1Script.stepInstructions[0]);
			step1Script.newRound = true;
			updatedRound = true;
		}

		myChuck.GetFloat ("pos", myGetPosCallback);
		//full loop has passed!!!
		if (myPos >= previousPos + 0.95f) {

			//check if both steps done
			if (step1Script.bottomDone == true && step1Script.topDone == true) {
				if (currRound == 3) {
					myChuck.RunCode ("0 => Global.introGain;");
				}
				myChuck.BroadcastEvent ("gotCorrect");
				if (currRound == 5) {
					myChuck.RunCode ("0.5 => Global.bassGain;");
				}
				if (currRound == 6) {
					myChuck.RunCode ("0.7 => Global.synthGain;");
				}
				if (currRound == 7) {
					myChuck.RunCode ("0.4 => Global.synthGain;");
					myChuck.RunCode ("0.7 => Global.synthGain2;");
				}
				if (currRound == 8) {
					myChuck.RunCode (@"[67,68,70,68] @=> Global.synthMelody;
									[70,72,74,72] @=> Global.synthMelody2;
									[51,51,58,58] @=> Global.bassMelody;");
				}
				if (currRound == 9) {
					myChuck.RunCode (@"[66,67,69,67] @=> Global.synthMelody;
									[69,71,73,71] @=> Global.synthMelody2;
									[50,50,57,57] @=> Global.bassMelody;");
				}
				if (currRound == 10) {
					myChuck.RunCode (@"0.0 => Global.synthGain;
									0.0 => Global.synthGain2;");
				}
				if (currRound == 11) {
					myChuck.RunCode (@"0 => Global.synthGain;
									0 => Global.synthGain2;");
				}

			}
		}

		if (myPos >= previousPos + 1.0f) {
			previousPos = previousPos + 1.0f;

			if (currRound < specialWords.GetLength(0)) {
				currRound++;
				updatedRound = false;
			}
			Debug.Log ("Current Round: " + currRound);
		}
		float distanceMultiplier = 1.5f;
		step1Script.linePos = (myPos - previousPos)*distanceMultiplier;

	}

	string[] oneD(int index1, int index2) {
		string[] oneDArray = new string[4];
		oneDArray [0] = specialWords [index1, index2, 0];
		oneDArray [1] = specialWords [index1, index2, 1];
		oneDArray [2] = specialWords [index1, index2, 2];
		oneDArray [3] = specialWords [index1, index2, 3];

		return oneDArray;
	}

	void getKey(){
		foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
			if (Input.GetKeyDown (vKey)) {
				//if ("Return" == vKey.ToString ()) {
					myChuck.RunCode (@"
						public class Global {
							static float synthGain;
							static float synthGain2;
			    			static float bassGain;
							static float introGain;

			    			static int synthMelody[];
			    			static int synthMelody2[];
			    			static int bassMelody[];
						}

						external Event gotCorrect;

					 	8 => external float timeStep;
						external float pos;

						fun void updatePos() {
							timeStep::second => dur currentTimeStep;
							currentTimeStep / 1000 => dur deltaTime;
							now => time startTime;
							
							pos => float originalPos;
											
							while( now < startTime + currentTimeStep )
							{
								deltaTime / currentTimeStep +=> pos;
								deltaTime => now;
							}
						}

						[66,67,69,67] @=> Global.synthMelody;
						[69,71,73,71] @=> Global.synthMelody2;
						[50,50,57,57] @=> Global.bassMelody;


						0 => Global.synthGain;
						0 => Global.synthGain2;
						0 => Global.bassGain;

						SinOsc synth => ADSR e => Gain localSynthGain => dac;
						SinOsc synth2 => Gain localSynthGain2 => dac;
						SinOsc bass => Gain localBassGain => dac;
						0 => synth.freq;
						0 => synth2.freq;
						0 => bass.freq;	

						200::ms => e.attackTime;
						100::ms => e.decayTime;
						.5 => e.sustainLevel;
						200::ms => e.releaseTime;
						1 => e.keyOn;

						Gain localIntroGain;
						.3 => Global.introGain;
						.3 => localIntroGain.gain;
						1 => int firstTime;
						fun void playIntroMelody(){
							// sound file
							me.sourceDir() + ""IntroMusicShort.wav"" => string filename;
							<<< filename >>>;
							if( me.args() ) me.arg(0) => filename;						
							// the patch 
							SndBuf buf => localIntroGain => dac;
							0 => buf.pos;

							// load the file
							filename => buf.read;

							buf.length() => now;	
						}
	
						fun void playMelody() {
							for (0 => int i; i < timeStep; i++) {
							    for (0 => int x; x < Global.synthMelody.cap(); x++)
							    {
							        Global.synthMelody[x] => Std.mtof => synth.freq;
							        125::ms => now;
							        0 => synth.freq;
							        125::ms => now;
							    }
							}
						}

						fun void playMelody2() {
						    for (0 => int i; i < timeStep; i++) {
						        for (0 => int x; x < Global.synthMelody2.cap(); x++)
						        {
						            Global.synthMelody2[x] => Std.mtof => synth2.freq;
						            125::ms => now;
						            0 => synth2.freq;
						            125::ms => now;
						        }
						    }
						}
						
						fun void playBass() {
							for (0 => int i; i < timeStep; i++) {
							    for (0 => int x; x < Global.bassMelody.cap(); x++)
							    {
							        Global.bassMelody[x] => Std.mtof => bass.freq;
							        250::ms => now;

							    }
							}
						}

					    TriOsc correct => Gain correctGain => dac;
						.04 => correctGain.gain;
						0 => correct.freq;

						//play if they get a step correct
						fun void playCorrect() {
							gotCorrect => now;
						    50 => Std.mtof => correct.freq;
						    100::ms => now;
						    53 => Std.mtof => correct.freq;
						    100::ms => now;
						    58 => Std.mtof => correct.freq;
						    100::ms => now;
							0 => correct.freq;
						}
								
						while( true )
						{
							Global.synthGain => localSynthGain.gain;
							Global.synthGain2 => localSynthGain2.gain;

							Global.bassGain => localBassGain.gain;
							Global.introGain => localIntroGain.gain;
							spork ~ updatePos();

							//ALL MUSIC PLAYS BELOW IN SEQUENCE
							if(firstTime == 1){
								0 => firstTime;
								spork ~ playIntroMelody();
							}
							spork ~ playMelody();
							spork ~ playBass();
							spork ~ playMelody2();
							spork ~ playCorrect();
							50::ms => now; //delay to make playCorrect not trigger twice
							timeStep::second => now;				
						}
					");
				//}

			}
		}
	}

	void GetPosCallback( System.Double pos )
	{
		myPos = (float) pos;
	}
}

