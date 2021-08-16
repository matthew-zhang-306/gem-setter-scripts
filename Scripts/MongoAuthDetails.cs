public class MongoAuthDetails {
    private const string DB_USERNAME = "gemDbUser";
    private const string DB_PASSWORD = "yoinkisabigfatthingthatdefinitelyexists_megan10000";
    public static readonly string[] DB_PATH = new string[] {"GemData", "GemCollection"};
    public static readonly string DB_URI = $"mongodb+srv://{MongoAuthDetails.DB_USERNAME}:{MongoAuthDetails.DB_PASSWORD}@gemdb-3yn7x.mongodb.net/{DB_PATH[0]}?retryWrites=true&w=majority";

    private const string DC_USERNAME = "gammaDc";
    private const string DC_PASSWORD = "if_you_are_reading_this_and_trying_to_break_my_game_then_i_want_you_to_know_that_i_am_very_disappointed_in_you_specifically";
    public static readonly string[] DC_PATH = new string[] {"SadData", "SadCollection"};
    public static readonly string DC_URI = $"mongodb+srv://{MongoAuthDetails.DC_USERNAME}:{MongoAuthDetails.DC_PASSWORD}@cluster0.o5if6.mongodb.net/{DC_PATH[0]}?retryWrites=true&w=majority";
}