
public static class UserInfo {

    public static string UserId = "";
    public static bool IsMale;
    public static bool IsNative;
    public static int Age;

    public static int Hit; //hit number where we can derive personality, initialqind and animind
    public static int Personality;
    public static int GroupInd; //the group index of the first question to be asked. Grouped as 0-7, 8-15, 16-23, 24-31, 32-39, 40-47
    public static int AnimInd; // 0 = pointing 1 = picking up
    public const int QCnt = 8;
    public static string PersonalityQuestion;
    public static int Quality = 0;

    
    private static string[] _personalityString = {
        "MORE \"open to new experiences & complex \", and LESS \"conventional & uncreative\"?",
        "MORE \"dependable & self-disciplined \", and LESS \"disorganized & careless\"?",
        "MORE \"extraverted & enthusiastic\", and LESS \"reserved & quiet\"?",
        "MORE \"sympathetic & warm\", and LESS \"critical & quarrelsome\"?",
        "MORE \"calm & emotionally stable\", and LESS \"anxious & easily upset\"?"
    };

    //Compute personality, initialquestionind and animInd from the hit number


    //HitCnt = AnimCnt* QCnt * personalityCnt
    public static void ComputeQuestionInfo() {

        //according to 1st hit index = 1
        int groupCnt = 48 / QCnt; //6 groups
        GroupInd = (Hit - 1) % groupCnt;

        AnimInd = ((Hit - 1) / groupCnt) % 2;

        Personality = ((Hit - 1) / groupCnt / 2) % 5;

        PersonalityQuestion = _personalityString[Personality];


    }



}
