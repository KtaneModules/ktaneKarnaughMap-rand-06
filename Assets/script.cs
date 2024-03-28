using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script : MonoBehaviour {

    public KMSelectable button1, button2, button3, button4, buttonD, buttonS;
    public TextMesh ScreenText, NotText;
    public KMBombModule Module;
    public TextMesh[] Coordinates;

    private static int _moduleIdCounter = 1;
    private int _moduleId = 0;
    private bool _isSolved = false, _lightsOn = false;
    private string internalSol = "";
    const int solLength = 16;

    bool CheckAnswer(bool isZero = false)
    {
        //internalSol += '+';
        string iteration = "";
        int finalMask = 0;
        if (!isZero)
        {
            for (int i = 0; i < internalSol.Length; i++)
            {
                if (internalSol[i] != '+')
                {
                    iteration += internalSol[i];
                }
                else
                {
                    int mask = 0xFFFF;
                    bool[] check = { false, false, false, false };
                    for (int ii = 0; ii < iteration.Length; ii++)
                    {
                        switch ((int)iteration[ii] - 48)
                        {
                            case 1:
                                {
                                    if (check[0]) {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                            }
                                    mask = mask & 0x3333;
                                    check[0] = true;
                                    break;
                                }
                            case 2:
                                {
                                    if (check[1])
                                    {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                    }
                                    mask = mask & 0x6666;
                                    check[1] = true;
                                    break;
                                }
                            case 3:
                                {
                                    if (check[2])
                                    {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                    }
                                    mask = mask & 0x00FF;
                                    check[2] = true;
                                    break;
                                }
                            case 4:
                                {
                                    if (check[3])
                                    {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                    }
                                    mask = mask & 0x0FF0;
                                    check[3] = true;
                                    break;
                                }
                            case 6:
                                {
                                    if (check[3])
                                    {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                    }
                                    mask = mask & 0xF00F;
                                    check[3] = true;
                                    break;
                                }
                            case 7:
                                {
                                    if (check[2])
                                    {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                    }
                                    mask = mask & 0xFF00;
                                    check[2] = true;
                                    break;
                                }
                            case 8:
                                {
                                    if (check[1])
                                    {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                    }
                                    mask = mask & 0x9999;
                                    check[1] = true;
                                    break;
                                }
                            case 9:
                                {
                                    if (check[0])
                                    {
                                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have repeating symbols in group.", _moduleId);
                                        return false;
                                    }
                                    mask = mask & 0xCCCC;
                                    check[0] = true;
                                    break;
                                }
                        }
                    }
                    bool ones = false;
                    for (int ii = 0; ii < 16 && !ones; ii++) if (((mask & 1 << (15 - ii)) != 0) && (Coordinates[ii].text == "1")) ones = true;
                    if (!ones) {
                        Debug.LogFormat("[Karnaugh Map #{0}] Strike. Can't have group only of \"*\".", _moduleId);
                        return false;
                            }
                    finalMask = finalMask | mask;
                    iteration = "";
                }
            }
        }
        int player1 = 0; // * is 1
        int player0 = 0; // * is 0
        for (int ii = 0; ii < 16; ii++)
        {
            player1 = player1 << 1;
            player1 = player1 | (Coordinates[ii].text == "0" ? 0 : 1);
            player0 = player0 << 1;
            player0 = player0 | (Coordinates[ii].text == "1" ? 1 : 0);
        }

        if ((((finalMask & ~player1) & 0xFFFF) == 0) && (((player0 & ~finalMask) & 0xFFFF) == 0))
        {
            Debug.LogFormat("[Karnaugh Map #{0}] Correct.", _moduleId);
            return true;
        }
        else
        {
            Debug.LogFormat("[Karnaugh Map #{0}] Strike. Wrong solution.", _moduleId);
            return false;
        }
    }
    void UpdateScreen()
    {
        
        string scr = "", not = "";
        for (int i=0; i<internalSol.Length; i++)
        {
            if (internalSol[i] != '+')
            {
                scr += internalSol[i] > '5' ? (char)('j' - internalSol[i]) : internalSol[i];
                
                not += internalSol[i] > '5' ? '_' : ' ';
            }
            else
            {
                scr += '+';
                not += ' ';
            }
        }
        ScreenText.text = scr;
        NotText.text = not;
        
        //ScreenText.text = internalSol;
    }
    bool HandlePressD()
    {
        if (_lightsOn && !_isSolved)
        {
            if (internalSol != "")
            {
                string int1 = "";
                for (int i = 0; i < internalSol.Length - 1; i++)
                {
                    int1 += internalSol[i];
                }
                internalSol = int1;
                UpdateScreen();
            }
        }
        return false;
    }
    bool HandlePressS()
    {
        if (_lightsOn && !_isSolved)
        {
            if (internalSol == "")
            {
                if (CheckAnswer(true))
                {
                    ScreenText.text = "Solved!";
                    NotText.text = "";
                    _isSolved = true;
                    Module.HandlePass();
                }
                else Module.HandleStrike();
            }
            else
            {
                if (internalSol[internalSol.Length - 1] != '+' && internalSol.Length < solLength)
                {
                    internalSol += '+';
                    UpdateScreen();
                }
                else
                {
                    if (CheckAnswer())
                    {
                        ScreenText.text = "Solved!";
                        NotText.text = "";
                        _isSolved = true;
                        Module.HandlePass();
                    }
                    else Module.HandleStrike();
                }
            }
        }
        return false;
    }
    bool HandlePress1()
    {
        if (_lightsOn && !_isSolved && internalSol.Length<20)
        {
            if (internalSol == "") internalSol += '1';
            else
            if (internalSol[internalSol.Length - 1] == '1')
            {
                HandlePressD();
                internalSol += '9';
            }
            else if (internalSol[internalSol.Length - 1] == '9')
            {
                HandlePressD();
                internalSol += '1';
            }
            else internalSol += '1';
            UpdateScreen();
        }
        return false;
    }
    bool HandlePress2()
    {
        if (_lightsOn && !_isSolved && internalSol.Length < solLength)
        {
            if (internalSol == "") internalSol += '2';
            else
        if (internalSol[internalSol.Length - 1] == '2')
            {
                HandlePressD();
                internalSol += '8';
            }
            else if (internalSol[internalSol.Length - 1] == '8')
            {
                HandlePressD();
                internalSol += '2';
            }
            else internalSol += '2';
            UpdateScreen();
        }
        return false;
    }
    bool HandlePress3()
    {
        if (_lightsOn && !_isSolved && internalSol.Length < solLength)
        {
            if (internalSol == "") internalSol += '3';
            else
        if (internalSol[internalSol.Length - 1] == '3')
            {
                HandlePressD();
                internalSol += '7';
            }
            else if (internalSol[internalSol.Length - 1] == '7')
            {
                HandlePressD();
                internalSol += '3';
            }
            else internalSol += '3';
            UpdateScreen();
        }
        return false;
    }
    bool HandlePress4()
    {
        if (_lightsOn && !_isSolved && internalSol.Length < solLength)
        {
            if (internalSol == "") internalSol += '4';
            else
        if (internalSol[internalSol.Length - 1] == '4')
            {
                HandlePressD();
                internalSol += '6';
            }
            else if (internalSol[internalSol.Length - 1] == '6')
            {
                HandlePressD();
                internalSol += '4';
            }
            else internalSol += '4';
            UpdateScreen();
        }
        return false;
    }

    void Activate()
    {
        _lightsOn = true;
        ScreenText.text = "";
        return;
    }

    char[,] GenerateSolution(){
        char[,] table = new char[4, 4];
        int ones = 0, zeroes = 0;
        for (int i = 0; i < 16; i++) {
            int r = Random.Range(-1, 2);
            if ((r == 1) && (ones<4))
            {
                table[i / 4, i % 4] = '1';
                ones++;
            }
            else if ((r == 0) && (zeroes < 4))
            {
                table[i / 4, i % 4] = '0';
                zeroes++;
            }
            else table[i / 4, i % 4] = '*';
        }
        return table;
    }

	// Use this for initialization
	void Start () {
        _moduleId = _moduleIdCounter++;
        ScreenText.text = "Karnaugh Map";
        NotText.text = "";
        Module.OnActivate += Activate;
        char[,] sol = GenerateSolution();
        Debug.LogFormat("[Karnaugh Map #{0}] Generated map: {1} {2} {3} {4} | {5} {6} {7} {8} | {9} {10} {11} {12} | {13} {14} {15} {16}", _moduleId,
            sol[0, 0], sol[0, 1], sol[0, 2], sol[0, 3],
            sol[1, 0], sol[1, 1], sol[1, 2], sol[1, 3],
            sol[2, 0], sol[2, 1], sol[2, 2], sol[2, 3],
            sol[3, 0], sol[3, 1], sol[3, 2], sol[3, 3]
            );

        for (int i = 0; i < 16; i++)
        {
            Coordinates[i].text = sol[i/4, i%4].ToString();
        }

        button1.OnInteract += HandlePress1;
        button2.OnInteract += HandlePress2;
        button3.OnInteract += HandlePress3;
        button4.OnInteract += HandlePress4;
        buttonD.OnInteract += HandlePressD;
        buttonS.OnInteract += HandlePressS;
        
        
    }

    // Update is called once per frame
    void Update () {
		
	}
}
