using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


public class DataSorterValidation : MonoBehaviour {
    private List<UserComparisonData> _userData = new List<UserComparisonData>();
    private List<UserComparisonData> _userDemographics = new List<UserComparisonData>();
    private List<string> _userNames = new List<string>();
    private List<UserComparisonData> _userList = new List<UserComparisonData>();

    private List<UserComparisonData> _usersToDelete = new List<UserComparisonData>();

    public void Start() {
        int gender, nativity;
        gender = (int)Gender.all;
        nativity = (int)Nativity.all;
/*
          AddUserInfoToAccept("allUsers.txt");
        ParseData(); //males, nativeAll
         WriteSortedFile();

         for (int p = 0; p < 5; p++) {
             string outFile = GetPersonalityName(p) + "_" + GetGenderName(gender) + "_" + GetNativityName(nativity) + ".txt"; //Assign outFile name
             StreamWriter sw = new StreamWriter(outFile);
            
             for (int a = 0; a < 2; a++) {
                
                 for (int c = 0; c < 3; c++) {
                     sw.WriteLine(GetAnimName(a) + " " + GetCharacterName(c));                    
                      ClassifyData(sw, p, a, c);
                      if (c == 0 || c == 2) {
                          sw.WriteLine();
                          sw.WriteLine();
                      }
                      else {
                          sw.WriteLine();
                          sw.WriteLine();
                          sw.WriteLine();
                      }
        
                    
                 }
             }
             sw.Close();
         }
         
  */      
        GetDemographics();
    }

    public void AddUserInfoToAccept(string fileName) {
        using (var reader = new StreamReader(File.OpenRead(fileName))) {
            reader.ReadLine();
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                string[] tokens = line.Split(',');
                UserComparisonData du = new UserComparisonData();
                
                du.UserId = tokens[0];
                du.Hit = int.Parse(tokens[1]);


                _userList.Add(du);
            }

        }
    }


    public void ParseData()
        {

            /*  using (var reader = new StreamReader(File.OpenRead("userHitsToDelete.txt"))) {
            reader.ReadLine();
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                string[] tokens = line.Split('\t');
                UserComparisonData du = new UserComparisonData();
                du.UserId = tokens[0];
                du.Hit = int.Parse(tokens[1]);
                _usersToDelete.Add(du);
            }
        }
        
        //Add more users to delete
        switch (gender) {
            case (int)Gender.male: {
        //only males            
                using (var reader = new StreamReader(File.OpenRead("femaleUsers.txt"))) {
                    
                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine();
                        string[] tokens = line.Split('\t');
                        UserComparisonData du = new UserComparisonData();
                        
                            if (!_usersToDelete.Any(x => (x.UserId.Equals(tokens[0]) && x.Hit == int.Parse(tokens[1])))) {
                            du.UserId = tokens[0];
                            du.Hit = int.Parse(tokens[1]);
                            _usersToDelete.Add(du);
                            
                        }
                    }
                }
                break;
            }

            case (int)Gender.female: {
                    //only males            
                    using (var reader = new StreamReader(File.OpenRead("maleUsers.txt"))) {
                    
                        while (!reader.EndOfStream) {
                            string line = reader.ReadLine();
                            string[] tokens = line.Split('\t');
                            UserComparisonData du = new UserComparisonData();
                            if (!_usersToDelete.Any(x => (x.UserId.Equals(tokens[0]) && x.Hit == int.Parse(tokens[1])))) {
                                //not already in the list
                                du.UserId = tokens[0];
                                du.Hit = int.Parse(tokens[1]);
                                _usersToDelete.Add(du);
                            }
                        }
                    }
                }
                break;
            default: //do not delete any
                break;
        }

        //Add more users to delete
        switch (nativity) {
            case (int)Nativity.native: {
                    //only natives            
                    using (var reader = new StreamReader(File.OpenRead("nonNativeUsers.txt"))) {
                        
                        while (!reader.EndOfStream) {
                            string line = reader.ReadLine();
                            string[] tokens = line.Split('\t');
                            UserComparisonData du = new UserComparisonData();
                            if (!_usersToDelete.Any(x => (x.UserId.Equals(tokens[0]) && x.Hit == int.Parse(tokens[1])))) {
                                //not already in the list
                                du.UserId = tokens[0];
                                du.Hit = int.Parse(tokens[1]);
                                _usersToDelete.Add(du);
                            }
                        }
                    }
                    break;
                }

            case (int)Nativity.nonNative: {
                    //only nonNatives            
                    using (var reader = new StreamReader(File.OpenRead("nativeUsers.txt"))) {                        
                        while (!reader.EndOfStream) {
                            string line = reader.ReadLine();
                            string[] tokens = line.Split('\t');
                            UserComparisonData du = new UserComparisonData();
                            if (!_usersToDelete.Any(x => (x.UserId.Equals(tokens[0]) && x.Hit == int.Parse(tokens[1])))) {
                                //not already in the list
                                du.UserId = tokens[0];
                                du.Hit = int.Parse(tokens[1]);
                                _usersToDelete.Add(du);
                            }
                        }
                    }
                }
                break;
            default: //do not delete any
                break;
        }
        */
            using (var reader = new StreamReader(File.OpenRead("userComparisonDataUnsorted.txt"))) {
                reader.ReadLine(); //header line as userId hit qind answer actualQInd

                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split('\t');
                    UserComparisonData ud = new UserComparisonData();
                    ud.UserId = tokens[0];
                    
                    ud.Hit = int.Parse(tokens[1]);
                    ud.ActualQInd = ud.Hit*5 + int.Parse(tokens[2]) - 1;
                    ud.Answer = int.Parse(tokens[3]);
                    //if (!ToBeDeleted(ud)) {
                    if (_userList.Exists(x => (x.UserId.Equals(ud.UserId) && x.Hit == ud.Hit))) {
                        
                        _userData.Add(ud);

                        
                        if (!_userNames.Contains(ud.UserId))
                            _userNames.Add(ud.UserId);

                    }
                }


            }
        }

    private bool ToBeDeleted(UserComparisonData userCheck) {
        return _usersToDelete.Any(ud => userCheck.UserId.Equals(ud.UserId) && userCheck.Hit == ud.Hit);
    }

    public void WriteSortedFile() {
        string outFile = "userComparisonDataSorted.txt";

        StreamWriter sw = new StreamWriter(outFile);

        sw.Write("ActualQInd");
        //First line
        foreach (string user in _userNames) {
            sw.Write("\t" + user);
        }
        sw.WriteLine();

        for (int i = 0; i < 45; i++) {
            //actual q ind            
            sw.Write(i);
            //Find user data with qind i
            foreach (string userId in _userNames) {
                //int answer = FindUserData(userId, i);
                int answer = _userData.Find(x => (x.UserId.Contains(userId) && x.ActualQInd == i)).Answer;
 


                sw.Write("\t" + answer);
            }
            sw.WriteLine();

        }
        sw.Close();

    }

    private int FindUserData(string userId, int actualQInd) {

        foreach (UserComparisonData ud in _userData) {
            if (ud.UserId.Equals(userId) && ud.ActualQInd == actualQInd)
                return ud.Answer;
        }
        return 0;

    }

    private string GetGenderName(int g) {
        switch (g) {
            case (int)Gender.male:
                return "M";
            case (int)Gender.female:
                return "F";
            default:
                return "All";
            
        }
    }

    private string GetNativityName(int n) {
        switch (n) {
            case (int)Nativity.native:
                return "Native";
            case (int)Nativity.nonNative:
                return "Non";
            default:
                return "All";
            
        }
    }
    private string GetPersonalityName(int p) {
        switch (p) {
            case 0:
                return "Openness";
            case 1:
                return "Conscientiousness";
            case 2:
                return "Extroversion";
            case 3:
                return "Agreeableness";
            case 4:
                return "Stability";
            default:
                return "PersonalityNameError";
        }
    }

    private string GetAnimName(int a) {
        switch (a) {
            case 0:
                return "Pointing";
            case 1:
                return "Throwing";
            case 2:
                return "Walking";
            default:
                return "AnimNameError";
        }
    }

    private string GetCharacterName(int c) {
        switch (c) {
            case 0:
                return "Chuck";
            case 1:
                return "Carl";
            case 2:
                return "Mia";            
            default:
                return "EffortNameError";
        }
    }

    //personality = [0 4] as OCEAN, animInd = [0 2] as point throw walk, character = [0 2] as chuck carl mia
    public void ClassifyData(StreamWriter sw,  int personality, int animInd, int character) {
        int qStart, qEnd;        

        //Compute start and end numbers of questions for personality and animInd
        qStart = personality*96 + animInd*48;
        qEnd = qStart + 47;

        string[] content = File.ReadAllLines("userComparisonDataSorted.txt");

        sw.WriteLine(content[0]);
        //Read from sorted file and write each personality x animation as a separate file
        for (int i = qStart; i <= qEnd; i++) {
            if (i%3 == character)
                sw.WriteLine(content[i + 1]);
        }

        
        //sw.Close();


    }


    public void GetDemographics() {
        using (var reader = new StreamReader(File.OpenRead("allUsers.txt"))) {
            reader.ReadLine(); //header line as userId hit qind answer actualQInd

            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                string[] tokens = line.Split(',');
                UserComparisonData ud = new UserComparisonData();
                ud.UserId = tokens[0];
                ud.Age = int.Parse(tokens[2]);
                ud.Gender = tokens[3];
                ud.Nativity = tokens[5];
                if (!_userDemographics.Contains(ud))
                    _userDemographics.Add(ud);
            }
        }

        string outFile = "userInfoSorted.txt";
        StreamWriter sw = new StreamWriter(outFile);
        
        for (int i = 0; i < _userDemographics.Count; i++) {
            sw.WriteLine(_userDemographics[i].UserId + "\t" + _userDemographics[i].Age + "\t" +
                         _userDemographics[i].Gender + "\t" + _userDemographics[i].Nativity);
        }


    sw.Close();
    }
}
