using System.Collections.Generic;
using UnityEngine;

namespace BomBomLemon.Game.Topics
{
    [CreateAssetMenu(fileName = "TopicDatabase", menuName = "BomBom Lemon/Topic Database", order = 1)]
    public class TopicDatabase : ScriptableObject
    {
        [SerializeField] private List<Topic> topics = new();

        public IReadOnlyList<Topic> Topics => topics;

        public Topic GetRandom()
        {
            if (topics.Count == 0) return FallbackTopic();
            return topics[Random.Range(0, topics.Count)];
        }

        static Topic FallbackTopic() => new(
            "好きな食べ物の甘さ", "甘くない", "めちゃ甘い", "塩", "砂糖の塊",
            "Sweetness of your fav food", "Not sweet", "Extremely sweet", "Salt", "Pure sugar"
        );

        [ContextMenu("Load Default Japanese Topics")]
        void LoadDefaultTopics()
        {
            topics = new List<Topic>
            {
                // 1
                new("誕生日にもらって嬉しいもの",
                    "全く嬉しくない", "最高に嬉しい", "割り箸1本", "無人島",
                    "Things you're happy to receive for your birthday",
                    "Not happy at all", "Overjoyed", "1 chopstick", "A private island"),
                // 2
                new("空腹のときに食べたいもの",
                    "食欲ゼロ", "最高においしい", "消しゴム", "神の料理",
                    "Things you want to eat when starving",
                    "Zero appetite", "Absolutely delicious", "An eraser", "Food of the gods"),
                // 3
                new("夏に涼しくなれるもの・こと",
                    "全然涼しくない", "極限まで涼しい", "うちわ1扇", "南極移住",
                    "Things that cool you down in summer",
                    "Not cool at all", "Freezing cold", "One fan wave", "Move to Antarctica"),
                // 4
                new("子どものころに欲しかったもの",
                    "全然欲しくなかった", "死ぬほど欲しかった", "消しゴムのかす", "時間停止能力",
                    "Things you wanted as a child",
                    "Didn't want at all", "Desperately wanted", "Eraser shavings", "Time-stop power"),
                // 5
                new("旅行先として行きたい場所",
                    "絶対行きたくない", "今すぐ行きたい", "隣の駅", "宇宙ステーション",
                    "Places you want to visit as a travel destination",
                    "Never want to go", "Want to go right now", "The next station", "A space station"),
                // 6
                new("疲れたときに癒されるもの・こと",
                    "全然癒されない", "完全に癒される", "深呼吸1回", "永遠の眠り",
                    "Things that heal you when you're exhausted",
                    "Not relaxing at all", "Fully restored", "One deep breath", "Eternal sleep"),
                // 7
                new("朝起きるのが楽しみになるもの",
                    "全然楽しみじゃない", "最高に楽しみ", "目覚まし時計", "宝くじ当選通知",
                    "Things that make you excited to wake up in the morning",
                    "Dread waking up", "Can't wait to get up", "An alarm clock", "Lottery win notice"),
                // 8
                new("無人島に持っていったら役立つもの",
                    "全く役に立たない", "命が救われる", "マッチ棒1本", "核融合発電機",
                    "Things useful to bring to a deserted island",
                    "Useless", "Life-saving", "One matchstick", "Nuclear fusion generator"),
                // 9
                new("友達に紹介したいもの・こと",
                    "紹介したくない", "絶対紹介したい", "空気", "不老不死の薬",
                    "Things you want to recommend to friends",
                    "Would never recommend", "Must share with everyone", "Air", "Immortality potion"),
                // 10
                new("寒い冬に温まれるもの・こと",
                    "全然温まらない", "極限まで温かい", "靴下1枚", "溶岩風呂",
                    "Things that warm you up in cold winter",
                    "No warmth at all", "Scorching hot", "One sock", "Lava bath"),
                // 11
                new("お腹が痛いときに助かるもの",
                    "全然助からない", "一瞬で治る", "深呼吸", "瞬間治癒薬",
                    "Things that help when you have a stomachache",
                    "No help at all", "Heals instantly", "A deep breath", "Instant cure pill"),
                // 12
                new("子どもに人気がありそうなもの",
                    "全然人気ない", "爆発的に人気", "請求書", "無限お菓子機",
                    "Things that seem popular with kids",
                    "Kids hate it", "Kids go crazy for it", "An invoice", "Infinite candy machine"),
                // 13
                new("大人になってよかったと感じる嬉しいこと",
                    "全然よくない", "最高によかった", "自分で歯磨き", "好きな時間に寝る",
                    "Good things about being an adult",
                    "Not good at all", "So glad to be an adult", "Brushing own teeth", "Sleep whenever you want"),
                // 14
                new("雨の日に気分が上がるもの・こと",
                    "全然上がらない", "最高に上がる", "濡れた靴", "虹色の竜巻",
                    "Things that lift your mood on a rainy day",
                    "Makes mood worse", "Best mood ever", "Wet shoes", "Rainbow tornado"),
                // 15
                new("運動会で盛り上がる競技",
                    "全然盛り上がらない", "最高に盛り上がる", "正座耐久", "全員ジェット噴射",
                    "Sports-day events that get everyone excited",
                    "Dead silent", "Crowd goes wild", "Sitting still contest", "Everyone has jetpacks"),
                // 16
                new("もらって嬉しいサプライズ",
                    "全然嬉しくない", "号泣するほど嬉しい", "塩1袋", "月を買ってもらう",
                    "Surprises you'd be happy to receive",
                    "Not happy at all", "Cry with joy", "A bag of salt", "Someone buys you the moon"),
                // 17
                new("ペットにするとかわいいいきもの",
                    "全然かわいくない", "最高にかわいい", "ミミズ", "ミニ恐竜",
                    "Creatures that would be cute as pets",
                    "Not cute at all", "Absolutely adorable", "An earthworm", "A mini dinosaur"),
                // 18
                new("老後に楽しめそうなもの・こと",
                    "全然楽しめない", "最高に楽しめる", "税金の計算", "時間旅行",
                    "Things you could enjoy in retirement",
                    "Not enjoyable", "Best time of your life", "Calculating taxes", "Time travel"),
                // 19
                new("引っ越し先として住みやすそうな場所",
                    "全然住みたくない", "すぐ引っ越したい", "活火山の火口", "天空の城",
                    "Places that seem easy and pleasant to live in",
                    "Would never live there", "Moving in right now", "Active volcano crater", "A sky castle"),
                // 20
                new("記念日に食べたいごちそう",
                    "全然食べたくない", "最高においしい", "水", "神が作った料理",
                    "Dishes you'd want for a special occasion",
                    "Not appetizing at all", "Best meal ever", "Water", "A dish cooked by god"),
                // 21
                new("試験勉強がはかどるもの・環境",
                    "全然はかどらない", "10倍速で覚えられる", "爆音ライブ会場", "記憶転送装置",
                    "Environments or things that help you study for exams",
                    "Zero focus", "Learn 10x faster", "Loud concert venue", "Memory transfer device"),
                // 22
                new("プレゼントとして贈りやすいもの",
                    "絶対贈れない", "誰でも喜ぶ", "使用済み歯ブラシ", "無限お金",
                    "Things easy to give as a gift",
                    "Can never give this", "Everyone loves it", "Used toothbrush", "Infinite money"),
                // 23
                new("子どもに読み聞かせたい絵本のテーマ",
                    "絶対読まない", "毎晩読みたい", "税務申告", "空を飛ぶ冒険",
                    "Themes for picture books to read to children",
                    "Would never read", "Read it every night", "Tax filing", "Flying adventure"),
                // 24
                new("長い列に並んで待ってでも食べたい食べ物",
                    "並ぶ気ゼロ", "3日並んでも食べたい", "白湯", "一生に一度の味",
                    "Food worth waiting in a long line for",
                    "Not worth any wait", "Worth a 3-day wait", "Boiled water", "Once-in-a-lifetime taste"),
                // 25
                new("雨の日の楽しい過ごし方",
                    "全然楽しくない", "最高に楽しい", "濡れながら外出", "雨粒でオーケストラ",
                    "Fun ways to spend a rainy day",
                    "Not fun at all", "Best day ever", "Go outside and get soaked", "Rain-drop orchestra"),
                // 26
                new("デートで行きたい場所",
                    "絶対行きたくない", "今すぐ行きたい", "税務署", "空飛ぶ宮殿",
                    "Places you'd want to go on a date",
                    "Never going on a date there", "Going right now", "Tax office", "A flying palace"),
                // 27
                new("家にあったら便利なもの",
                    "全然便利じゃない", "生活が激変する", "砂利", "瞬間移動装置",
                    "Things that would be convenient to have at home",
                    "Not convenient at all", "Changes your life", "Gravel", "Teleportation device"),
                // 28
                new("夜ご飯に食べたいもの",
                    "食欲ゼロ", "最高においしい", "紙", "宇宙一の料理",
                    "Things you want to eat for dinner",
                    "Zero appetite", "Absolutely delicious", "Paper", "Best food in the universe"),
                // 29
                new("子どもに習わせたいスキル・習い事",
                    "全然習わせたくない", "絶対習わせたい", "税金の納め方", "超能力",
                    "Skills or lessons you'd want your child to learn",
                    "Would never teach this", "Must learn this", "How to pay taxes", "Superpower"),
                // 30
                new("ストレス解消になるもの・こと",
                    "全然解消されない", "一瞬で消える", "深呼吸1回", "全部爆破する",
                    "Things that relieve stress",
                    "No relief at all", "Stress gone instantly", "One deep breath", "Blow everything up"),
                // 31
                new("キャンプで役立つもの",
                    "全く役立たない", "なければ死ぬ", "ネクタイ", "無限薪マシン",
                    "Things useful for camping",
                    "Completely useless", "Can't survive without it", "A necktie", "Infinite firewood machine"),
                // 32
                new("体調不良のときに食べやすいもの",
                    "絶対食べられない", "一瞬で元気になる", "生唐辛子", "神の薬膳",
                    "Things easy to eat when you're feeling sick",
                    "Impossible to eat", "Recover instantly", "Raw chili pepper", "Divine medicinal food"),
                // 33
                new("100円で買える嬉しいもの",
                    "全然嬉しくない", "信じられないほど嬉しい", "輪ゴム1本", "タイムマシン",
                    "Things you'd be happy to buy for 100 yen",
                    "Not happy at all", "Unbelievably happy", "One rubber band", "A time machine"),
                // 34
                new("働くモチベーションが上がるもの・こと",
                    "全然上がらない", "無限に働ける", "追加の仕事", "給料10億円",
                    "Things that boost your motivation to work",
                    "No motivation boost", "Could work forever", "More tasks added", "1 billion yen salary"),
                // 35
                new("部屋に飾りたいもの",
                    "絶対飾りたくない", "最高に映える", "ゴミ袋", "生きた滝",
                    "Things you'd want to decorate your room with",
                    "Would never display this", "Looks incredible", "A trash bag", "A living waterfall"),
                // 36
                new("眠れない夜に役立つもの・こと",
                    "全然役立たない", "一瞬で眠れる", "コーヒー10杯", "神の子守唄",
                    "Things that help when you can't sleep",
                    "Makes it worse", "Asleep in seconds", "10 cups of coffee", "A lullaby from god"),
                // 37
                new("子どもに人気の遊び",
                    "全然人気ない", "取り合いになる", "確定申告ごっこ", "本物の魔法",
                    "Games and activities popular with kids",
                    "Kids hate it", "Kids fight over it", "Tax filing role-play", "Real magic"),
                // 38
                new("老後にやってみたいこと",
                    "全然やりたくない", "絶対やりたい", "窓拭き", "宇宙移住",
                    "Things you'd want to try in retirement",
                    "Never want to do", "Absolutely must do", "Wiping windows", "Moving to space"),
                // 39
                new("コンビニで買える嬉しいもの",
                    "全然嬉しくない", "最高に嬉しい", "袋", "当たり付き宝くじ",
                    "Things you'd be happy to buy at a convenience store",
                    "Not happy at all", "Best feeling ever", "A plastic bag", "A winning lottery ticket"),
                // 40
                new("長距離ドライブのお供になるもの",
                    "全然役立たない", "ドライブが最高になる", "無音", "自動運転AI",
                    "Things that make a long road trip better",
                    "No help at all", "Best drive ever", "Complete silence", "Self-driving AI"),
                // 41
                new("お花見で盛り上がるもの・こと",
                    "全然盛り上がらない", "最高に盛り上がる", "無言で花見", "桜が喋り出す",
                    "Things that make cherry blossom viewing exciting",
                    "No excitement", "Best party ever", "Silent flower viewing", "The sakura start talking"),
                // 42
                new("一人で楽しめるもの・こと",
                    "全然楽しめない", "永遠に楽しめる", "壁を見る", "並行宇宙探索",
                    "Things you can enjoy alone",
                    "Not enjoyable solo", "Could do it forever", "Staring at the wall", "Exploring parallel universes"),
                // 43
                new("子どもの誕生日パーティーで喜ばれるもの",
                    "全然喜ばれない", "大爆発の喜び", "請求書", "本物の竜",
                    "Things kids would love at a birthday party",
                    "Kids hate it", "Kids go absolutely wild", "An invoice", "A real dragon"),
                // 44
                new("海外旅行で買いたいお土産",
                    "絶対買いたくない", "絶対買って帰りたい", "砂", "現地の神話",
                    "Souvenirs you'd want to buy on a trip abroad",
                    "Would never buy", "Must bring home", "Sand", "The local mythology"),
                // 45
                new("運動不足解消になるもの・こと",
                    "全然解消されない", "完璧に解消", "寝返り", "重力10倍トレーニング",
                    "Things that help fix lack of exercise",
                    "No effect", "Perfectly fit", "Rolling over in bed", "10x gravity training"),
                // 46
                new("二人で食べると美味しいもの",
                    "一人で食べた方がいい", "二人で食べると倍旨い", "ガム", "愛の鍋",
                    "Food that tastes better when shared with someone",
                    "Better alone", "Twice as delicious together", "Gum", "A pot of love"),
                // 47
                new("家族で楽しめるもの・こと",
                    "家族で絶対やらない", "家族の絆が深まる", "確定申告", "全員テレパシー",
                    "Things the whole family can enjoy together",
                    "Family would never do this", "Deepens family bonds", "Tax filing", "Everyone gets telepathy"),
                // 48
                new("友達と盛り上がれるゲーム",
                    "全然盛り上がらない", "夜通し続ける", "足し算問題", "現実が変わるゲーム",
                    "Games that get exciting with friends",
                    "No excitement", "Play all night", "Addition problems", "A game that changes reality"),
                // 49
                new("夏祭りで楽しいもの",
                    "全然楽しくない", "最高に楽しい", "ゴミ拾い", "花火が喋る",
                    "Fun things at a summer festival",
                    "Not fun at all", "Best night ever", "Picking up trash", "The fireworks start talking"),
                // 50
                new("寝る前にすると眠れるもの・こと",
                    "全然眠れない", "3秒で熟睡", "エスプレッソ", "神が歌う",
                    "Things that help you fall asleep before bed",
                    "Makes you wide awake", "Asleep in 3 seconds", "An espresso", "God sings you to sleep"),
                // 51
                new("二日酔いのときに食べやすいもの",
                    "絶対食べられない", "一瞬で回復", "生ニンニク", "魔法の回復薬",
                    "Food easy to eat when hungover",
                    "Impossible to eat", "Recover instantly", "Raw garlic", "A magic recovery potion"),
                // 52
                new("子どもが喜ぶおやつ",
                    "全然喜ばない", "泣いて喜ぶ", "にがり", "無限お菓子",
                    "Snacks that kids love",
                    "Kids hate it", "Kids cry with joy", "Nigari (bittern)", "Infinite snacks"),
                // 53
                new("忙しいときに助かるもの・サービス",
                    "全然助からない", "時間が生まれる", "追加タスク", "時間を止める能力",
                    "Things or services that help when you're busy",
                    "Makes it worse", "Creates extra time", "More tasks", "Ability to stop time"),
                // 54
                new("冬に食べたい温かいもの",
                    "全然温まらない", "体の芯まで温まる", "氷", "溶岩鍋",
                    "Warm foods you want to eat in winter",
                    "No warmth", "Warm to the core", "Ice", "Lava hot pot"),
                // 55
                new("日曜の朝に楽しいもの・こと",
                    "全然楽しくない", "最高に幸せ", "5時起きで仕事", "朝から温泉",
                    "Enjoyable things on a Sunday morning",
                    "Not enjoyable", "Pure bliss", "Wake at 5am for work", "Hot spring from the morning"),
                // 56
                new("初対面の人と仲良くなれるもの・こと",
                    "むしろ遠ざかる", "一瞬で親友", "無言", "心が読める能力",
                    "Things that help you bond with someone you just met",
                    "Pushes people away", "Instant best friends", "Complete silence", "Ability to read minds"),
                // 57
                new("困ったときに頼りになる人の特徴",
                    "全然頼りにならない", "完璧に助けてくれる", "「知らん」と言う人", "何でも解決する人",
                    "Traits of a reliable person when you're in trouble",
                    "Not reliable at all", "Solves everything", "Says 'Not my problem'", "Fixes anything instantly"),
                // 58
                new("ハイキングに持っていくと役立つもの",
                    "全く役立たない", "命が救われる", "ヒール靴", "無限エネルギー機",
                    "Things useful to bring on a hike",
                    "Completely useless", "Life-saving", "High heels", "Infinite energy machine"),
                // 59
                new("記憶に残る嬉しい体験",
                    "すぐ忘れる", "一生忘れない", "信号待ち", "宇宙で目覚める",
                    "Happy experiences that stay in your memory",
                    "Forget immediately", "Remember forever", "Waiting at a traffic light", "Waking up in space"),
                // 60
                new("料理が得意な人が作ってくれると嬉しいもの",
                    "全然嬉しくない", "感動して泣く", "湯冷まし", "神の晩餐",
                    "Dishes you'd love a good cook to make for you",
                    "Not happy at all", "Moved to tears", "Warm water", "A divine dinner"),
                // 61
                new("気分転換になるもの・こと",
                    "全然気分転換にならない", "完全リフレッシュ", "同じ部屋で深呼吸", "別次元に飛ぶ",
                    "Things that help you change your mood",
                    "No change at all", "Completely refreshed", "Deep breath in same room", "Jump to another dimension"),
                // 62
                new("道に迷ったときに助かるもの",
                    "全然助からない", "一瞬で解決", "白紙の地図", "全方位GPS脳",
                    "Things that help when you're lost",
                    "No help at all", "Solved instantly", "Blank map", "360-degree GPS brain"),
                // 63
                new("子どもが夢中になれるもの",
                    "全然夢中にならない", "時間を忘れる", "確定申告書", "本物の魔法学校",
                    "Things kids get completely absorbed in",
                    "Kids ignore it", "Kids lose track of time", "A tax return form", "A real magic school"),
                // 64
                new("プロポーズで感動できるもの・こと",
                    "むしろ引く", "一生の宝物", "レシートを渡す", "星に名前をつける",
                    "Proposal gestures that are truly moving",
                    "Creepy or off-putting", "Treasure it for life", "Hand them a receipt", "Name a star after them"),
                // 65
                new("老人ホームで楽しめるもの・こと",
                    "全然楽しめない", "毎日やりたい", "税金の計算", "若返り体験",
                    "Things elderly people can enjoy at a care home",
                    "Not enjoyable", "Want to do it every day", "Calculating taxes", "Experience growing young"),
                // 66
                new("二人でいると楽しい時間",
                    "一人の方がいい", "永遠にいたい", "黙って座る", "時間が止まる",
                    "Time that's fun when spent with someone",
                    "Better alone", "Want it to last forever", "Sitting in silence", "Time freezes"),
                // 67
                new("コスパが良くて嬉しいもの・サービス",
                    "完全に損", "信じられないお得", "10万円のガム", "永遠に使える道具",
                    "Things or services with great value for money",
                    "Total rip-off", "Unbelievable bargain", "100k yen gum", "A tool that lasts forever"),
                // 68
                new("子どもに残してあげたいもの",
                    "全然残したくない", "最高の遺産", "ガラクタ", "並行宇宙のパスポート",
                    "Things you'd want to leave behind for your children",
                    "Would never leave this", "The greatest legacy", "Junk", "A parallel universe passport"),
                // 69
                new("外食で頼みたいもの",
                    "絶対頼みたくない", "毎回頼む", "水のみ", "食べると幸せになる料理",
                    "Dishes you'd want to order at a restaurant",
                    "Would never order", "Order it every time", "Just water", "A dish that brings happiness"),
                // 70
                new("春に楽しめるもの・こと",
                    "全然楽しめない", "最高に楽しい", "花粉を数える", "花と会話する",
                    "Things you can enjoy in spring",
                    "Not enjoyable", "Best season ever", "Counting pollen grains", "Talking to flowers"),
                // 71
                new("インドア派が楽しめるもの・こと",
                    "全然楽しめない", "永遠に楽しめる", "壁を見る", "仮想宇宙を作る",
                    "Things indoor people can enjoy",
                    "Not enjoyable indoors", "Could do it forever", "Staring at the wall", "Building a virtual universe"),
                // 72
                new("アウトドア派が楽しめるもの・こと",
                    "全然楽しめない", "永遠に楽しめる", "ソファで寝る", "エベレスト登頂",
                    "Things outdoor lovers can enjoy",
                    "Not enjoyable outside", "Could do it forever", "Napping on the sofa", "Climbing Everest"),
                // 73
                new("仕事終わりに嬉しいもの・こと",
                    "全然嬉しくない", "最高に幸せ", "残業追加", "給料2倍通知",
                    "Things you're happy about after finishing work",
                    "Not happy at all", "Absolute bliss", "More overtime added", "Double salary notification"),
                // 74
                new("寒い日に外出したくなるもの・こと",
                    "絶対外出しない", "すぐ飛び出す", "税務署呼び出し", "奇跡の雪祭り",
                    "Things that make you want to go outside on a cold day",
                    "Would never go out", "Rush outside instantly", "Tax office summons", "A miracle snow festival"),
                // 75
                new("停電の夜に楽しめること",
                    "全然楽しくない", "最高に楽しい", "暗闇で正座", "星空の下で踊る",
                    "Things you can enjoy during a blackout at night",
                    "Not fun at all", "Best night ever", "Sitting still in the dark", "Dancing under the stars"),
                // 76
                new("カラオケで盛り上がれるもの・こと",
                    "全然盛り上がらない", "最高に盛り上がる", "無音で立つ", "声が光になる",
                    "Things that make karaoke exciting",
                    "Total silence", "Party goes wild", "Standing in silence", "Your voice becomes light"),
                // 77
                new("誕生日パーティーで盛り上がる出し物",
                    "全然盛り上がらない", "爆発的に盛り上がる", "領収書の読み上げ", "本物の竜が現れる",
                    "Performances that hype up a birthday party",
                    "Kills the vibe", "Crowd explodes", "Reading receipts aloud", "A real dragon appears"),
                // 78
                new("家の中でできる楽しいこと",
                    "全然楽しくない", "永遠にできる", "壁を数える", "部屋が宇宙になる",
                    "Fun things you can do inside the house",
                    "Not fun at all", "Could do it forever", "Counting the walls", "Your room becomes space"),
                // 79
                new("風邪をひいたときに助かるもの",
                    "全然助からない", "即座に完治", "生ニンニク", "神の治癒光線",
                    "Things that help when you have a cold",
                    "No help at all", "Cured instantly", "Raw garlic", "A divine healing ray"),
                // 80
                new("勉強のやる気が出るもの・こと",
                    "やる気ゼロ", "10倍の集中力", "追加の宿題", "記憶転送装置",
                    "Things that motivate you to study",
                    "Zero motivation", "10x focus", "More homework added", "Memory transfer device"),
                // 81
                new("友達と過ごす休日に楽しいこと",
                    "全然楽しくない", "最高の休日", "黙って座る", "別世界を旅する",
                    "Fun things to do with friends on a day off",
                    "Not fun at all", "Best holiday ever", "Sitting in silence", "Traveling to another world"),
                // 82
                new("学校や会社に行くのが楽しみになる嬉しいこと",
                    "全然楽しみじゃない", "飛び起きる", "追加課題の通知", "全員に昇給通知",
                    "Things that make you excited to go to school or work",
                    "Dread going", "Jump out of bed", "Extra assignment notice", "Everyone gets a raise"),
                // 83
                new("旅行の思い出として嬉しいもの",
                    "全然嬉しくない", "一生の宝物", "レシート", "時間を持ち帰る",
                    "Things you'd be happy to have as travel memories",
                    "Not happy at all", "Treasure it for life", "A receipt", "Bringing back time itself"),
                // 84
                new("年末年始に楽しめるもの・こと",
                    "全然楽しくない", "最高に楽しい", "確定申告作業", "時間が2倍になる",
                    "Things you can enjoy over New Year",
                    "Not enjoyable", "Best time of year", "Tax return work", "Time doubles"),
                // 85
                new("一人旅で嬉しいもの・こと",
                    "全然嬉しくない", "最高の体験", "迷子", "世界が変わる出会い",
                    "Happy things about traveling alone",
                    "Not happy at all", "Life-changing experience", "Getting lost", "A life-changing encounter"),
                // 86
                new("運動会のお昼ご飯として嬉しいもの",
                    "全然嬉しくない", "最高においしい", "水のみ", "五つ星シェフの弁当",
                    "Happy lunches to have at a school sports day",
                    "Not happy at all", "Absolutely delicious", "Just water", "A 5-star chef's bento"),
                // 87
                new("暇つぶしになるもの・こと",
                    "全然暇つぶしにならない", "時間を忘れる", "天井を見る", "別宇宙を作る",
                    "Things that help pass the time",
                    "No help at all", "Lose track of time", "Staring at the ceiling", "Building another universe"),
                // 88
                new("引っ越しのとき助かるもの・こと",
                    "全然助からない", "一瞬で終わる", "段ボール1枚", "瞬間移動装置",
                    "Things that help when moving to a new place",
                    "No help at all", "Done in an instant", "One cardboard box", "Teleportation device"),
                // 89
                new("子育てで楽しい瞬間",
                    "全然楽しくない", "最高の幸福", "おむつ替え深夜", "子が空を飛ぶ",
                    "Joyful moments in raising a child",
                    "Not enjoyable", "Greatest joy in life", "Midnight diaper change", "Your child flies"),
                // 90
                new("悲しいときに元気が出るもの・こと",
                    "むしろ落ち込む", "即座に元気100%", "さらに悲しい話", "神が抱きしめる",
                    "Things that cheer you up when you're sad",
                    "Makes it worse", "100% energy instantly", "An even sadder story", "God gives you a hug"),
                // 91
                new("仕事で達成感があるもの・こと",
                    "全然達成感ない", "最高の達成感", "消耗品補充", "世界を救う",
                    "Work tasks that give a sense of accomplishment",
                    "No satisfaction", "Greatest satisfaction ever", "Restocking supplies", "Saving the world"),
                // 92
                new("記念日のサプライズとして嬉しいもの",
                    "全然嬉しくない", "号泣するほど嬉しい", "割り箸1膳", "宇宙に名前を刻む",
                    "Surprises you'd love to receive on an anniversary",
                    "Not happy at all", "Cry with joy", "One set of chopsticks", "Your name carved in space"),
                // 93
                new("二日酔いのときに助かるもの",
                    "全然助からない", "即回復", "追加の酒", "時間を戻す薬",
                    "Things that help when you're hungover",
                    "Makes it worse", "Recover instantly", "More alcohol", "A pill to rewind time"),
                // 94
                new("秋に楽しめるもの・こと",
                    "全然楽しくない", "最高に楽しい", "落ち葉を数える", "紅葉が音楽を奏でる",
                    "Things you can enjoy in autumn",
                    "Not enjoyable", "Best season ever", "Counting fallen leaves", "Autumn leaves play music"),
                // 95
                new("お風呂上がりに嬉しいもの・こと",
                    "全然嬉しくない", "最高に幸せ", "冷水シャワー", "全身マッサージ付き",
                    "Happy things after getting out of the bath",
                    "Not happy at all", "Absolute bliss", "Cold water shower", "Full-body massage included"),
                // 96
                new("寝坊した朝に助かるもの・こと",
                    "全然助からない", "時間が戻る", "渋滞情報", "時間停止能力",
                    "Things that help when you've overslept",
                    "No help at all", "Time reverses", "Traffic update", "Ability to stop time"),
                // 97
                new("お正月に嬉しいもの・こと",
                    "全然嬉しくない", "最高に幸せ", "税金の通知", "無限お年玉",
                    "Happy things about New Year",
                    "Not happy at all", "Pure happiness", "A tax notice", "Infinite New Year money"),
                // 98
                new("冬のデートで楽しいもの・場所",
                    "全然楽しくない", "一生の思い出", "税務署前", "雪の宮殿",
                    "Fun things or places for a winter date",
                    "Not fun at all", "Memory for life", "In front of the tax office", "A palace of snow"),
                // 99
                new("大切な人への気持ちを伝えるもの・方法",
                    "全然伝わらない", "完璧に伝わる", "レシートを渡す", "魂ごと渡す",
                    "Ways to express your feelings to someone special",
                    "Feeling not conveyed", "Perfectly understood", "Hand them a receipt", "Give them your soul"),
                // 100
                new("人生を振り返って一番嬉しかった体験",
                    "全く嬉しくなかった", "最高に嬉しかった", "ゴミ袋が安かった", "宇宙で奇跡を体験",
                    "The happiest experience when you look back on life",
                    "Not happy at all", "The greatest joy in life", "Cheap trash bags", "A miracle in space"),
            };
        }
    }
}
