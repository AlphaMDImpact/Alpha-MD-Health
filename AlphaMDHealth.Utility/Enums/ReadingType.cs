namespace AlphaMDHealth.Utility;

/// <summary>
/// Reading type
/// </summary>
public enum ReadingType
{
    /// <summary>
    /// None
    /// </summary>
    None = 0,

    /// <summary>
    /// Height
    /// </summary>
    Height = 655,

    /// <summary>
    /// Weight
    /// </summary>
    Weight = 654,

    /// <summary>
    /// Steps
    /// </summary>
    Steps = 692,

    /// <summary>
    /// Heart rate
    /// </summary>
    HeartRate = 667,

    /// <summary>
    /// Blood pressure
    /// </summary>
    BloodPressure = 656,

    /// <summary>
    /// Blood pressure systolic
    /// </summary>
    BPSystolic = 657,

    /// <summary>
    /// Blood pressure diastolic
    /// </summary>
    BPDiastolic = 658,

    /// <summary>
    /// Blood glucose
    /// </summary>
    BloodGlucose = 659,

    /// <summary>
    /// Oxygeneted haemoglobin
    /// </summary>
    SpO2 = 674,

    /// <summary>
    /// Body fat
    /// </summary>
    BodyFat = 900,

    /// <summary>
    /// Calories consumed
    /// </summary>
    CaloriesConsumed = 11,

    /// <summary>
    /// Calories expended
    /// </summary>
    CaloriesExpended = 12,

    /// <summary>
    /// Distance covered by walking running
    /// </summary>
    DistanceWalkingRunning = 13,

    /// <summary>
    /// Distance covered by cycling
    /// </summary>
    DistanceCycling = 697,

    /// <summary>
    /// Hydration
    /// </summary>
    Hydration = 681,

    /// <summary>
    /// Body temperature
    /// </summary>
    BodyTemperature = 668,

    /// <summary>
    /// 
    /// </summary>
    CholesterolTotal = 19,

    /// <summary>
    /// 
    /// </summary>
    LDLCholesterol = 20,

    /// <summary>
    /// 
    /// </summary>
    HDLCholesterol = 21,

    /// <summary>
    /// 
    /// </summary>
    ForcedExpiratoryVolume1Second = 22,

    /// <summary>
    /// 
    /// </summary>
    Albumin = 743,

    /// <summary>
    /// 
    /// </summary>
    AlbuminCreatinineRatio = 24,

    /// <summary>
    /// 
    /// </summary>
    HeartRateResting = 25,

    /// <summary>
    /// 
    /// </summary>
    BSE = 26,

    /// <summary>
    /// 
    /// </summary>
    CRP = 27,

    /// <summary>
    /// 
    /// </summary>
    Creatinine = 747,

    /// <summary>
    /// 
    /// </summary>
    AntiCCP = 29,

    /// <summary>
    /// 
    /// </summary>
    CholesterolHDLRatio = 30,

    /// <summary>
    /// 
    /// </summary>
    eGFR_CKD_EPI = 31,

    /// <summary>
    /// 
    /// </summary>
    eGFR_MDRD = 32,

    /// <summary>
    /// 
    /// </summary>
    Kalium = 33,

    /// <summary>
    /// 
    /// </summary>
    CreatinineUrine = 716,

    /// <summary>
    /// 
    /// </summary>
    CreatinineClearanceCockcroft = 35,

    /// <summary>
    /// 
    /// </summary>
    LDL_HDLCholesterolRatio = 36,

    /// <summary>
    /// 
    /// </summary>
    Natrium = 37,

    /// <summary>
    /// 
    /// </summary>
    RheumatoidFactor = 38,

    /// <summary>
    /// 
    /// </summary>
    Triglycerides = 39,

    /// <summary>
    /// 
    /// </summary>
    RespiratoryRate = 40,

    //MoveMinutes = 16,
    //BasalBodyTemperature = 17,
    //BasalCaloriesBurned = 19,

    /// <summary>
    /// Sleep
    /// </summary>
    Sleep = 41,

    /// <summary>
    /// HBA1C
    /// </summary>
    HbA1c = 727,

    /// <summary>
    /// Glucose fasting
    /// </summary>
    GlucoseFasting = 662,

    /// <summary>
    /// Glucose after break fast
    /// </summary>
    BloodGlucoseAfterBreakfast = 663,

    /// <summary>
    /// Glucose before lunch
    /// </summary>
    BloodGlucoseBeforeLunch = 660,

    /// <summary>
    /// Glucose after lunch
    /// </summary>
    BloodGlucoseAfterLunch = 661,

    /// <summary>
    /// Glucose before dinner
    /// </summary>
    BloodGlucoseBeforeDiner = 664,

    /// <summary>
    /// Glucose after dinner
    /// </summary>
    BloodGlucoseAfterDiner = 665,

    /// <summary>
    /// Glucose before bed
    /// </summary>
    BloodGlucoseBeforeBed = 666,

    /// <summary>
    /// BMI
    /// </summary>
    BMI = 676,

    /// <summary>
    /// Vitals
    /// </summary>
    Vitals = 51,

    /// <summary>
    /// 
    /// </summary>
    GlucosePreMeal = 52,

    /// <summary>
    /// 
    /// </summary>
    GlucosePostMeal = 53,

    /// <summary>
    /// 
    /// </summary>
    BloodGlucoseRandom = 54,
    ////FEV1Percentage,
    ////FEV1FEV6RatioPredicted,
    ////FEV1FVCRatioPredicted,
    ////FEV1FVCRatioPostBD,
    ////FEV1PreBD,
    ////FEV1FVCRatioPreBD,

    /// <summary>
    /// Workout
    /// </summary>
    Workout = 424,

    /// <summary>
    /// Walking
    /// </summary>
    Walking = 708,

    /// <summary>
    /// Running
    /// </summary>
    Running = 703,

    /// <summary>
    /// Jogging
    /// </summary>
    Jogging = 204,

    /// <summary>
    /// Bicycling
    /// </summary>
    Bicycling = 697,

    /// <summary>
    /// Weights / Weightlifting
    /// </summary>
    Weightlifting = 206,

    /// <summary>
    /// Swimming
    /// </summary>
    Swimming = 707,

    /// <summary>
    /// Aerobics
    /// </summary>
    Aerobics = 208,

    /// <summary>
    /// Archery
    /// </summary>
    Archery = 209,

    /// <summary>
    /// Badminton
    /// </summary>
    Badminton = 693,

    /// <summary>
    /// Baseball
    /// </summary>
    Baseball = 211,

    /// <summary>
    /// Basketball
    /// </summary>
    Basketball = 212,

    /// <summary>
    /// Biathlon
    /// </summary>
    Biathlon = 213,

    /// <summary>
    /// Boxing
    /// </summary>
    Boxing = 694,

    /// <summary>
    /// Calisthenics
    /// </summary>
    Calisthenics = 215,

    /// <summary>
    /// CircuitTraining
    /// </summary>
    CircuitTraining = 216,

    /// <summary>
    /// Cricket
    /// </summary>
    Cricket = 217,

    /// <summary>
    /// Crossfit
    /// </summary>
    Crossfit = 696,

    /// <summary>
    /// Curling
    /// </summary>
    Curling = 219,

    /// <summary>
    /// Dancing
    /// </summary>
    Dancing = 698,

    /// <summary>
    /// Elliptical
    /// </summary>
    Elliptical = 221,

    /// <summary>
    /// Ergometer
    /// </summary>
    Ergometer = 222,

    /// <summary>
    /// Fencing
    /// </summary>
    Fencing = 223,

    /// <summary>
    /// FootballAmerican
    /// </summary>
    FootballAmerican = 224,

    /// <summary>
    /// FootballAustralian
    /// </summary>
    FootballAustralian = 225,

    /// <summary>
    /// Football / Soccer
    /// </summary>
    Soccer = 705,

    /// <summary>
    /// FrisbeeDisc
    /// </summary>
    FrisbeeDisc = 227,

    /// <summary>
    /// Gardening
    /// </summary>
    Gardening = 228,

    /// <summary>
    /// Golf
    /// </summary>
    Golf = 229,

    /// <summary>
    /// Gymnastics
    /// </summary>
    Gymnastics = 700,

    /// <summary>
    /// Handball
    /// </summary>
    Handball = 231,

    /// <summary>
    /// HighIntensityIntervalTraining
    /// </summary>
    HighIntensityIntervalTraining = 710,

    /// <summary>
    /// Hiking
    /// </summary>
    Hiking = 233,

    /// <summary>
    /// Hockey
    /// </summary>
    Hockey = 234,

    /// <summary>
    /// HorsebackRiding
    /// </summary>
    HorsebackRiding = 235,

    /// <summary>
    /// IceSkating
    /// </summary>
    IceSkating = 236,

    /// <summary>
    /// IntervalTraining
    /// </summary>
    IntervalTraining = 237,

    /// <summary>
    /// JumpRope
    /// </summary>
    JumpRope = 711,

    /// <summary>
    /// Kayaking
    /// </summary>
    Kayaking = 239,

    /// <summary>
    /// KettlebellTraining
    /// </summary>
    KettlebellTraining = 240,

    /// <summary>
    /// Kickboxing
    /// </summary>
    Kickboxing = 712,

    /// <summary>
    /// KickScooter
    /// </summary>
    KickScooter = 242,

    /// <summary>
    /// MartialArts
    /// </summary>
    MartialArts = 701,

    /// <summary>
    /// Meditation
    /// </summary>
    Meditation = 244,

    /// <summary>
    /// MixedMartialArts
    /// </summary>
    MixedMartialArts = 245,

    /// <summary>
    /// Paragliding
    /// </summary>
    Paragliding = 246,

    /// <summary>
    /// Pilates
    /// </summary>
    Pilates = 713,

    /// <summary>
    /// Polo
    /// </summary>
    Polo = 248,

    /// <summary>
    /// Racquetball
    /// </summary>
    Racquetball = 249,

    /// <summary>
    /// RockClimbing
    /// </summary>
    RockClimbing = 250,

    /// <summary>
    /// Rowing
    /// </summary>
    Rowing = 251,

    /// <summary>
    /// RowingMachine
    /// </summary>
    RowingMachine = 252,

    /// <summary>
    /// Rugby
    /// </summary>
    Rugby = 253,

    /// <summary>
    /// Sailing
    /// </summary>
    Sailing = 254,

    /// <summary>
    /// ScubaDiving
    /// </summary>
    ScubaDiving = 255,

    /// <summary>
    /// Skateboarding
    /// </summary>
    Skateboarding = 256,

    /// <summary>
    /// Skating
    /// </summary>
    Skating = 704,

    /// <summary>
    /// Skiing
    /// </summary>
    Skiing = 258,

    /// <summary>
    /// Snowboarding
    /// </summary>
    Snowboarding = 259,

    /// <summary>
    /// Softball
    /// </summary>
    Softball = 260,

    /// <summary>
    /// Squash
    /// </summary>
    Squash = 706,

    /// <summary>
    /// StairClimbing
    /// </summary>
    StairClimbing = 714,

    /// <summary>
    /// StandupPaddleboarding
    /// </summary>
    StandupPaddleboarding = 263,

    /// <summary>
    /// StrengthTraining
    /// </summary>
    StrengthTraining = 699,

    /// <summary>
    /// Surfing
    /// </summary>
    Surfing = 265,

    /// <summary>
    /// TableTennis
    /// </summary>
    TableTennis = 266,

    /// <summary>
    /// TeamSports
    /// </summary>
    TeamSports = 267,

    /// <summary>
    /// Tennis
    /// </summary>
    Tennis = 268,

    /// <summary>
    /// Treadmill
    /// </summary>
    Treadmill = 269,

    /// <summary>
    /// Volleyball
    /// </summary>
    Volleyball = 270,

    /// <summary>
    /// Yoga
    /// </summary>
    Yoga = 709,

    /// <summary>
    /// Zumba
    /// </summary>
    Zumba = 272,

    /// <summary>
    /// Hunting
    /// </summary>
    Hunting = 273,

    /// <summary>
    /// Fishing
    /// </summary>
    Fishing = 274,

    /// <summary>
    /// Lacrosse
    /// </summary>
    Lacrosse = 275,

    /// <summary>
    /// Wrestling
    /// </summary>
    Wrestling = 276,

    /// <summary>
    /// Climbing
    /// </summary>
    Climbing = 695,

    /// <summary>
    /// Stretching
    /// </summary>
    Stretching = 278,

    /// <summary>
    /// Stretching
    /// </summary>
    TrackAndField = 279,

    /// <summary>
    /// Barre
    /// </summary>
    Barre = 280,

    /// <summary>
    /// Cardio
    /// </summary>
    Cardio = 702,

    /// <summary>
    /// Cardio
    /// </summary>
    HandCycling = 282,

    /// <summary>
    /// WaterSports
    /// </summary>
    WaterSports = 283,

    /// <summary>
    /// Sledding
    /// </summary>
    Sledding = 284,

    /// <summary>
    /// Sledding
    /// </summary>
    P90X = 285,

    /// <summary>
    /// Sledding
    /// </summary>
    Housework = 286,

    /// <summary>
    /// Diving
    /// </summary>
    Diving = 287,

    /// <summary>
    /// WheelChair
    /// </summary>
    WheelChair = 288,

    /// <summary>
    /// SnowSports
    /// </summary>
    SnowSports = 289,

    /// <summary>
    /// Bowling
    /// </summary>
    Bowling = 290,

    /// <summary>
    /// CoreTraining
    /// </summary>
    CoreTraining = 291,

    /// <summary>
    /// StepTraining
    /// </summary>
    StepTraining = 292,

    /// <summary>
    /// All Others Workout Types
    /// </summary>
    Others = 299,

    /// <summary>
    /// Nutrition Type
    /// </summary>
    Nutrition = 553,

    /// <summary>
    /// Nutrition sodium
    /// </summary>
    NutritionSodium = 679,

    /// <summary>
    /// Nutrition sugar
    /// </summary>
    NutritionSugar = 682,

    /// <summary>
    /// Nutrition total carbs
    /// </summary>
    NutritionTotalCarbs = 683,

    /// <summary>
    /// Nutrition total fat
    /// </summary>
    NutritionTotalFat = 684,

    /// <summary>
    /// Nutrition saturated fat
    /// </summary>
    NutritionSaturatedFat = 306,

    /// <summary>
    /// Nutrition unsaturated fat
    /// </summary>
    NutritionUnsaturatedFat = 307,

    /// <summary>
    /// Nutrition mono saturated fat
    /// </summary>
    NutritionMonounsaturatedFat = 308,

    /// <summary>
    /// Nutrition poly unsaturated Fat
    /// </summary>
    NutritionPolyunsaturatedFat = 309,

    /// <summary>
    /// Nutrition trans fat
    /// </summary>
    NutritionTransFat = 310,

    /// <summary>
    /// Nutrition vitamin A
    /// </summary>
    NutritionVitaminA = 685,

    /// <summary>
    /// Nutrition vitamin C
    /// </summary>
    NutritionVitaminC = 686,

    /// <summary>
    /// Nutrition protein
    /// </summary>
    NutritionProtein = 678,

    /// <summary>
    /// Nutrition potassium
    /// </summary>
    NutritionPotassium = 687,

    /// <summary>
    /// Nutrition calcium
    /// </summary>
    NutritionCalcium = 688,

    /// <summary>
    /// Nutrition calories
    /// </summary>
    NutritionCalories = 680,

    /// <summary>
    /// Nutrition cholesterol
    /// </summary>
    NutritionCholesterol = 689,

    /// <summary>
    /// Nutrition dietary fiber
    /// </summary>
    NutritionDietaryFiber = 690,

    /// <summary>
    /// Nutrition iron
    /// </summary>
    NutritionIron = 691,

    /// <summary>
    /// Nutrition zinc
    /// </summary>
    NutritionZinc = 320,
    /// <summary>
    /// Food Type
    /// </summary>
    Food = 401,
    /// <summary>
    /// Baby Kick Counter
    /// </summary>
    BabyKickCounter = 677,
    /// <summary>
    /// Vitamin D
    /// </summary>
    VitaminD = 902,
    /// <summary>
    /// Hip Circumference
    /// </summary>
    HipCircumference = 935,
    /// <summary>
    /// Wrist Circumference
    /// </summary>
    WristCircumference = 936,
    /// <summary>
    /// Forearm Circumference 
    /// </summary>
    ForearmCircumference = 937,
    /// <summary>
    /// Nappy Change Count
    /// </summary>
    NappyChangeCount = 941,
    /// <summary>
    /// Handgrip Strength
    /// </summary>
    HandgripStrength = 944,
    /// <summary>
    /// Near-Infrared Interactance
    /// </summary>
    NearInfraredInteractance = 950,
    /// <summary>
    /// BodyVolume
    /// </summary>
    BodyVolume = 953
}