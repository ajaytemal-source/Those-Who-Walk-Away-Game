public enum Trigger{
    //
    None = 0,
    IrysGlitch = 1,
    EricSmartass = 2,

    //Omelas Conversation
    NobodyDeservesIt = 3,
    EndsDontMeans = 4, 
    NeedNoUtopia = 5, 
    NeedNoOmni = 6, 
    HypotheticalsAreTricky = 7,


    //Clara asks how your converstaion went 
    IrysWasNice = 8, 
    DefinitelySomething = 9, 
    ItWasTerrible = 10, 
    PleasantConversation = 11, 
    //value 12 removed 
    IrysLikesHypotheticals = 13, 

    //
    YouNeedToHurry = 14, 


    //
    YouTrustHer = 15,
    NotThatFast = 16, 
    NoTrust = 17, 
    YouHelpHer = 18, 
    WhyMeHelp = 19, 
    ImSkeptical = 20,


    //
    YeahRight = 21, 
    SeeProblem = 22, 
    NoneMatters = 23,
    ThatsIt = 24,

    //
    DevynIsFriend = 49, 
    DevynIsPartner = 50, 
    DontKnowDevyn = 51,

    //
    WereFine = 25, 
    HoldOfMe = 26, 
    TrustMyVision = 27, 
    NothingsHere = 28,
    GreenEnough = 29, 

    //
    SendEricTerminalEmail = 30,

    //If you decide to ask Clara if someone/she asked out or not 
    ForcedAnswer = 31,
    MovedOn = 32,

    //In convo #20 after IRYS asks you why you sent that question 
    WhatYouMean = 33,
    NotMyQuestions = 34, 
    ContinueMentorship = 35,


    //For convo #21
    TookOffer = 36, 
    NoTakeOffer = 37,


    //For convo #22
    ThatsCorrect = 38, 
    ThinkIUseAcc = 39, 
    WhatUTalkBout = 40,

    IDidntDo = 41,
    IDidIt = 42,

    ProveNotMe = 43, 
    IUseTerminal = 44, 
    StillNoIdea = 45,

    DntKnowDevyn = 46, 
    IKnowDevyn = 47, 
    DevynNoAnswer = 48,

    IDidAlone = 52, 
    ItsIRYS = 53, 
    ICantSay = 54,

    IrysSide = 55, 
    RenesSide = 56,

    QueueClaraEricEmail = 57,

    ClaraIsRight = 58, 
    EricIsRight = 59, 
    IdkWtf = 60

}

public enum ConversationStall{
    None = 0, 
    IrysViaIRYS = 1,
    IrysViaET = 2,
    ClaraViaEM = 3, 
    ClaraViaET = 4, 
    EricViaEM = 5, 
    EricViaET = 6, 
    AishaViaEM = 7,
    RenesViaET = 8 

}

public enum TerminalTrigger{

    NoTrigger = -1,
    openTerminal = 0,
    connectCommandOne = 1, 
    scopeCommand = 2, 
    wrongPassword = 3, 
    rightPassword = 4, 
    connectCommandTwo = 5, 
    pipelineCommandOne = 6, 
    resetCommand = 7, 
    reloadCommand = 8, 
    pipelineCommandTwo = 9,
    activateInternet = 10

}
