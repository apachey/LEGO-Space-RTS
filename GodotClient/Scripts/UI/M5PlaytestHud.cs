using System.Text;
using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.UI;

public partial class M5PlaytestHud : CanvasLayer
{
    private static int s_requestedStep = 1;
    public static int RequestedStep => s_requestedStep;

    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private RtsCameraController? _camera;
    private Action? _restart;
    private Label? _explanation;
    private Label? _instruction;
    private Label? _status;
    private Button? _run;
    private Button? _stepAction;
    private int _step;
    private bool _completionPaused;
    private bool _surgeActivationPaused;
    private bool _tubeArrivalFocused;
    private bool _pauseOnFocus;
    private Vector3? _displacementCameraAnchor;
    private double _nextUpdate;
    private readonly StringBuilder _text = new(768);

    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsCameraController camera, Action restart, bool pauseInitially)
    {
        _bridge = bridge; _selection = selection; _camera = camera; _restart = restart;
        _step = Mathf.Clamp(s_requestedStep, 1, 6);
        _pauseOnFocus = pauseInitially;
        bridge.SimulationPaused = pauseInitially;
        Name = "M5PlaytestHUD"; Layer = 18; ProcessPriority = 205;

        PanelContainer panel = new()
        {
            Name = "M5AcceptancePanel",
            AnchorLeft = 0.60f, AnchorRight = 0.99f, AnchorTop = 0.025f, AnchorBottom = 0.78f,
            OffsetLeft = 0, OffsetRight = 0, OffsetTop = 0, OffsetBottom = 0
        };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.035f, 0.052f, 0.070f, 0.97f), BorderColor = new Color("e6ad28"),
            BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 7, CornerRadiusTopRight = 7, CornerRadiusBottomLeft = 7, CornerRadiusBottomRight = 7,
            ContentMarginLeft = 14, ContentMarginRight = 14, ContentMarginTop = 11, ContentMarginBottom = 11
        });
        VBoxContainer box = new(); box.AddThemeConstantOverride("separation", 7); panel.AddChild(box);
        box.AddChild(Label("M5 — ПОКРОКОВА ПЕРЕВІРКА", 19, new Color("e6ad28")));
        Label intro = Label("Кожна цифра відкриває свіжий окремий показ. Прочитай «ЩО ЦЕ», а потім виконай одну зелену інструкцію.", 13, new Color("aab4b8"));
        intro.AutowrapMode = TextServer.AutowrapMode.WordSmart; box.AddChild(intro);

        _explanation = Label(string.Empty, 14, new Color("f2eee3"));
        _explanation.Name = "M5Explanation"; _explanation.AutowrapMode = TextServer.AutowrapMode.WordSmart; box.AddChild(_explanation);
        _instruction = Label(string.Empty, 14, new Color("65c987"));
        _instruction.Name = "M5CurrentInstruction"; _instruction.AutowrapMode = TextServer.AutowrapMode.WordSmart; box.AddChild(_instruction);
        _status = Label(string.Empty, 14, new Color("f2eee3"));
        _status.Name = "M5AcceptanceStatus"; _status.AutowrapMode = TextServer.AutowrapMode.WordSmart; _status.SizeFlagsVertical = Control.SizeFlags.ExpandFill; box.AddChild(_status);

        GridContainer focuses = new() { Columns = 2 }; focuses.AddThemeConstantOverride("h_separation", 6); focuses.AddThemeConstantOverride("v_separation", 6); box.AddChild(focuses);
        AddStep(focuses, 1, "1. БАЗОВА МЕРЕЖА");
        AddStep(focuses, 2, "2. ЗМІНА РОЛІ T3");
        AddStep(focuses, 3, "3. ЗАРЯД І СПЛЕСК");
        AddStep(focuses, 4, "4. АЕРОТРУБА");
        AddStep(focuses, 5, "5. ЗАХИСТ ВІД ПОШТОВХІВ");
        AddStep(focuses, 6, "6. ПРОХІД У СКЕЛІ");

        _run = Button(pauseInitially ? "ЗАПУСТИТИ ЦЕЙ ТЕСТ" : "ПАУЗА");
        _run.Name = "ToggleM5Simulation"; _run.Visible = _step is 2 or 3 or 4;
        _run.Pressed += ToggleSimulation; box.AddChild(_run);
        _stepAction = Button(_step == 5 ? "ПОКАЗАТИ ПЕРШИЙ ПОШТОВХ" : "ВІДКРИТИ СКЕЛЬНУ СТІНУ");
        _stepAction.Name = "M5StepAction"; _stepAction.Visible = _step is 5 or 6;
        _stepAction.Pressed += PerformStepAction; box.AddChild(_stepAction);
        Button replay = Button("ПОВТОРИТИ ЦЕЙ ТЕСТ З ПОЧАТКУ"); replay.Name = "ReplayM5Step"; replay.Pressed += RestartCurrentStep; box.AddChild(replay);
        Button reset = Button("ПОВЕРНУТИСЯ ДО ТЕСТУ 1"); reset.Name = "RestartM5Acceptance"; reset.Pressed += () => SelectStep(1); box.AddChild(reset);
        AddChild(panel);
        Callable.From(FocusCurrentStep).CallDeferred();
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _status is null) return;
        MaintainDisplacementFraming();
        HandleGuidedCameraAndPauses();
        double now = Time.GetTicksMsec() / 1000.0;
        if (now < _nextUpdate) return;
        _nextUpdate = now + 0.15;
        _status.Text = BuildStatus(_bridge.World);
        if (_run is not null) _run.Text = _bridge.SimulationPaused ? "ЗАПУСТИТИ ЦЕЙ ТЕСТ" : "ПАУЗА";
    }

    private void AddStep(Container parent, int step, string text)
    {
        Button button = Button(text); button.Pressed += () => SelectStep(step); parent.AddChild(button);
    }

    private void SelectStep(int step)
    {
        s_requestedStep = step;
        _restart?.Invoke();
    }

    private void RestartCurrentStep()
    {
        s_requestedStep = _step;
        _restart?.Invoke();
    }

    private void ToggleSimulation()
    {
        if (_bridge is null) return;
        _completionPaused = false;
        _bridge.SimulationPaused = !_bridge.SimulationPaused;
    }

    private void PerformStepAction()
    {
        if (_bridge is null || _instruction is null || _stepAction is null) return;
        if (_step == 6)
        {
            if (!M5AcceptanceScenarioFactory.TryOpenExcavationAndMove(_bridge.World))
            {
                _instruction.Text = "Прохід уже відкритий або тест втратив початковий стан. Натисни «ПОВТОРИТИ ЦЕЙ ТЕСТ З ПОЧАТКУ».";
                return;
            }
            _bridge.RefreshPresentationNow();
            _bridge.SimulationPaused = false;
            _stepAction.Text = "СКЕЛЬНУ СТІНУ ВІДКРИТО";
            _stepAction.Disabled = true;
            _instruction.Text = "СТІНА ЗНИКЛА. Тепер Hover Scout їде зліва направо через щойно відкритий наземний прохід.";
            return;
        }

        if (_step != 5) return;
        _bridge.SimulationPaused = true;
        bool repeat = M5AcceptanceScenarioFactory.TryFindFirst(_bridge.World, M5AcceptanceScenarioFactory.DisplacementTargetKey, out EntityId target) &&
            DisplacementSystem.IsStable(_bridge.World, target);
        if (M5AcceptanceScenarioFactory.TryRepeatDisplacement(_bridge.World, out _))
        {
            _bridge.RefreshPresentationNow();
            if (repeat)
            {
                _instruction.Text = "ДРУГИЙ ПОШТОВХ: ціль пересунулась лише на 0,5 клітинки — рівно 25% першої сили. Ланцюг сильних поштовхів заблокований.";
                _stepAction.Text = "ОБИДВА ПОШТОВХИ ПОКАЗАНО";
                _stepAction.Disabled = true;
            }
            else
            {
                _instruction.Text = "ПЕРШИЙ ПОШТОВХ: ворожий T3-Trike помітно пересунувся на 2 клітинки й отримав 8 секунд захисту. Тепер натисни ще раз для слабкого повтору.";
                _stepAction.Text = "ПОКАЗАТИ ПОВТОРНИЙ ПОШТОВХ";
            }
        }
        else
            _instruction.Text = "Поштовх заблокований перешкодою. Перезапусти цей тест і спробуй ще раз.";
    }

    private void FocusCurrentStep()
    {
        if (_bridge is null || _explanation is null || _instruction is null) return;
        if (_pauseOnFocus) _bridge.SimulationPaused = true;
        _explanation.Text = Explanation(_step);
        _instruction.Text = Instruction(_step);
        if (_stepAction is not null) _stepAction.Visible = _step is 5 or 6;

        switch (_step)
        {
            case 1: Focus("building.rock_raiders.hq"); break;
            case 2: Focus(M5AcceptanceScenarioFactory.T3TrikeKey); break;
            case 3: Focus(M5AcceptanceScenarioFactory.ResonanceCoreKey); break;
            case 4: Focus(M5AcceptanceScenarioFactory.AeroTubeHangarKey); break;
            case 5: FocusDisplacementPair(); break;
            case 6: FocusExcavation(); break;
        }
    }

    private void Focus(string stableKey)
    {
        if (_bridge is null || !M5AcceptanceScenarioFactory.TryFindFirst(_bridge.World, stableKey, out EntityId entity)) return;
        _selection?.SetSelection(new[] { entity });
        if (_bridge.World.Entities.Transform.TryGet(entity, out SimTransform transform)) _camera?.CenterOn(transform.Position.ToWorld());
    }

    private void FocusDisplacementPair()
    {
        if (_bridge is null || !M5AcceptanceScenarioFactory.TryFindFirst(_bridge.World, M5AcceptanceScenarioFactory.DisplacementSourceKey, out EntityId source) ||
            !M5AcceptanceScenarioFactory.TryFindFirst(_bridge.World, M5AcceptanceScenarioFactory.DisplacementTargetKey, out EntityId target)) return;
        _selection?.SetSelection(new[] { source, target });
        if (_bridge.World.Entities.Transform.TryGet(source, out SimTransform a) && _bridge.World.Entities.Transform.TryGet(target, out SimTransform b))
        {
            _displacementCameraAnchor = ((a.Position + b.Position) * Fix32.Half).ToWorld();
            MaintainDisplacementFraming();
        }
    }

    private void MaintainDisplacementFraming()
    {
        if (_step != 5 || _camera is null || !_displacementCameraAnchor.HasValue) return;
        _camera.FrameGroundPointAtViewport(_displacementCameraAnchor.Value, new Vector2(0.28f, 0.38f), 24f);
    }

    private void FocusExcavation()
    {
        if (_bridge is null || !_bridge.World.Map.TryGetFeature(M5AcceptanceScenarioFactory.ExcavatableFeatureId, out ExcavatableFeature feature)) return;
        _selection?.SetSelection(Array.Empty<EntityId>());
        FixVec2 center = new(
            Fix32.FromRatio(feature.NavRect.X * 2 + feature.NavRect.Width, MapGrid.NavPerBuild * 2),
            Fix32.FromRatio(feature.NavRect.Y * 2 + feature.NavRect.Height, MapGrid.NavPerBuild * 2));
        _camera?.CenterOn(center.ToWorld());
    }

    private void HandleGuidedCameraAndPauses()
    {
        if (_bridge is null || _bridge.SimulationPaused || _completionPaused) return;
        SimulationWorld world = _bridge.World;
        if (_step == 2 && M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.T3TrikeKey, out EntityId t3) &&
            !world.Entities.MissionRefitJob.Has(t3) && world.Entities.MissionRefitState.Get(t3).CurrentConfiguration == MissionConfiguration.T3Survey)
        {
            PauseAtResult("ГОТОВО: T3 став розвідником. Модуль куплений назавжди для цієї машини; огляд зріс із 9 до 13, швидкість — з 1,70 до 1,85.");
        }
        else if (_step == 3 && !_surgeActivationPaused && M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.ResonanceCoreKey, out EntityId core) &&
            world.Entities.SurgeZone.TryGet(core, out SurgeZone zone) && zone.BuildupRemainingTicks == 0)
        {
            _surgeActivationPaused = true;
            _bridge.SimulationPaused = true;
            if (_instruction is not null) _instruction.Text = "СПЛЕСК АКТИВНИЙ: Resonance Core підсилив Razor Skimmer у радіусі 12 клітинок. Натисни «ЗАПУСТИТИ» ще раз, щоб побачити відновлення Заряду.";
        }
        else if (_step == 4 && CountTransfers(world) == 0)
        {
            if (!_tubeArrivalFocused)
            {
                _tubeArrivalFocused = true;
                Focus(M5AcceptanceScenarioFactory.SettlementStationKey);
            }
            PauseAtResult("ГОТОВО: камера перейшла до станції призначення. Усі три юніти знову на мапі й стоять зовні будівлі.");
        }
        else if (_step == 6 && M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.ExcavationRunnerKey, out EntityId runner) &&
            world.Entities.Transform.TryGet(runner, out SimTransform runnerTransform) && runnerTransform.Position.X >= Fix32.FromInt(62))
        {
            _selection?.SetSelection(new[] { runner });
            PauseAtResult("ГОТОВО: Hover Scout пройшов просто крізь колишню скельну стіну. Це новий наземний маршрут, але будувати на цій ділянці не можна.");
        }
    }

    private void PauseAtResult(string result)
    {
        if (_bridge is null) return;
        _bridge.SimulationPaused = true; _completionPaused = true;
        if (_instruction is not null) _instruction.Text = result;
    }

    private string BuildStatus(SimulationWorld world)
    {
        _text.Clear();
        _text.Append(_bridge!.SimulationPaused ? "СТАН: ПАУЗА\n" : "СТАН: ТЕСТ ІДЕ\n");
        switch (_step)
        {
            case 1: AppendWorksite(world); break;
            case 2: AppendRefit(world); break;
            case 3: AppendSurge(world); break;
            case 4: AppendTube(world); break;
            case 5: AppendDisplacement(world); break;
            case 6: AppendExcavation(world); break;
        }
        return _text.ToString().TrimEnd();
    }

    private void AppendWorksite(SimulationWorld world)
    {
        EntityId[] components = WorksiteGraphSystem.GetPlayerComponents(world, 0);
        _text.Append("Rock Raiders HQ + Сервісний ангар: ").Append(components.Length == 1 ? "ОДНА СПІЛЬНА МЕРЕЖА\n" : "НЕ З'ЄДНАНІ\n");
        _text.Append("Практичний результат: будівлі мають спільний доступ до 400 Ore та спільної Energy.");
    }

    private void AppendRefit(SimulationWorld world)
    {
        if (!M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.T3TrikeKey, out EntityId t3)) return;
        MissionRefitState state = world.Entities.MissionRefitState.Get(t3);
        _text.Append("Машина: T3-Trike\nПоточна роль: ").Append(state.CurrentConfiguration == MissionConfiguration.T3Survey ? "РОЗВІДКА" : "ЕСКОРТ").Append('\n');
        if (world.Entities.MissionRefitJob.TryGet(t3, out MissionRefitJob job))
            _text.Append("Переоснащення: ").Append((job.TotalTicks - job.RemainingTicks) * 100 / job.TotalTicks).Append("%\n");
        else _text.Append("Переоснащення: ЗАВЕРШЕНО\n");
        _text.Append("Модуль розвідки: ").Append((state.OwnedConfigurationMask & 2) != 0 ? "КУПЛЕНИЙ ЦІЄЮ МАШИНОЮ" : "ВІДКРИТИЙ, АЛЕ ЩЕ НЕ КУПЛЕНИЙ").Append('\n');
        _text.Append("Ефект ролі «Розвідка»: огляд 9 → 13; швидкість 1,70 → 1,85.");
    }

    private void AppendSurge(SimulationWorld world)
    {
        AlienChargeState charge = world.GetAlienCharge(0);
        _text.Append("Джерело: Resonance Core з 4 встановленими Crystals\n");
        _text.Append("Заряд: ").Append(Charge(charge.CurrentMillicharge)).Append(" / ").Append(Charge(charge.MaximumMillicharge)).Append("  •  відновлення +1,6/с\n");
        _text.Append("На запуск уже витрачено 50 Заряду зі 100. Тому лічильник знову росте.\n");
        if (M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.ResonanceCoreKey, out EntityId core) && world.Entities.SurgeZone.TryGet(core, out SurgeZone zone))
            _text.Append(zone.BuildupRemainingTicks > 0 ? "Сплеск: ПІДГОТОВКА " : "Сплеск: АКТИВНИЙ ")
                .Append((zone.BuildupRemainingTicks > 0 ? zone.BuildupRemainingTicks : zone.ActiveRemainingTicks) / 20.0f).Append("с\n");
        else _text.Append("Сплеск: ЗАВЕРШЕНО\n");
        bool boosted = M5AcceptanceScenarioFactory.TryFindFirst(world, "unit.ali.razor_skimmer", out EntityId receiver) && AlienChargeSystem.IsSurged(world, receiver);
        _text.Append("Razor Skimmer у зоні: ").Append(boosted ? "ПІДСИЛЕНИЙ" : "ЩЕ НЕ ПІДСИЛЕНИЙ").Append("\n");
        _text.Append("Ефект: атаки перезаряджаються на 20% швидше; трансформації ETX — на 30% швидше. Бойові постріли ще не видно, бо M4 Combat відкладений.");
    }

    private void AppendTube(SimulationWorld world)
    {
        _text.Append("Маршрут: Аеротрубний ангар → Поселення\n");
        AppendPassenger(world, M5AcceptanceScenarioFactory.WorkerRobotKey, "Worker Robot");
        AppendPassenger(world, M5AcceptanceScenarioFactory.DoubleHoverKey, "Double Hover");
        AppendPassenger(world, M5AcceptanceScenarioFactory.JetScooterKey, "Jet Scooter");
        _text.Append("Під час поїздки юніт навмисно зникає з мапи. Після прибуття камера сама перейде до виходу.");
    }

    private void AppendPassenger(SimulationWorld world, string stableKey, string name)
    {
        if (!M5AcceptanceScenarioFactory.TryFindFirst(world, stableKey, out EntityId passenger)) return;
        string state = world.Entities.TubeTransfer.TryGet(passenger, out TubeTransfer transfer)
            ? TransferState(transfer.State)
            : world.Entities.Transform.Has(passenger) ? "ПРИБУВ, ЗНОВУ НА МАПІ" : "ПОМИЛКА: НЕМАЄ НА МАПІ";
        _text.Append(name).Append(": ").Append(state).Append('\n');
    }

    private void AppendDisplacement(SimulationWorld world)
    {
        int seconds = 0;
        bool displaced = false;
        if (M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.DisplacementTargetKey, out EntityId target))
        {
            seconds = (DisplacementSystem.RemainingStabilityTicks(world, target) + 19) / 20;
            displaced = DisplacementSystem.IsStable(world, target);
        }
        _text.Append("Хто штовхає: Martian Excavation Searcher\n");
        _text.Append("Ціль: ворожий Astronaut T3-Trike\n");
        _text.Append(displaced ? "Перший поштовх: ЗАСТОСОВАНО — 2 клітинки. Захист цілі: " : "Перший поштовх: ЩЕ НЕ ЗАСТОСОВАНО. Захист цілі: ").Append(seconds).Append("с\n");
        _text.Append("Навіщо: протягом захисту наступні поштовхи мають лише 25% сили — ворога не можна нескінченно тримати в ланцюгу контролю.");
    }

    private void AppendExcavation(SimulationWorld world)
    {
        bool open = world.Map.TryGetFeature(M5AcceptanceScenarioFactory.ExcavatableFeatureId, out ExcavatableFeature feature) && feature.Open;
        _text.Append(open ? "Колишня перешкода: скельна стіна для НАЗЕМНИХ ЮНІТІВ\n" : "Перешкода ЗАРАЗ: видима скельна стіна для НАЗЕМНИХ ЮНІТІВ\n");
        _text.Append("Ділянка: ").Append(open ? "ВІДКРИТА ДЛЯ РУХУ" : "ЗАКРИТА").Append("\n");
        _text.Append(open ? "Перевірка: Hover Scout їде зліва направо через щойно відкриту сіру смугу. Прохід доступний усім фракціям, але не стає місцем для будівництва."
            : "Hover Scout стоїть ліворуч і не має маршруту крізь стіну. Натискання кнопки прибере скелю та дасть йому прямий наказ проїхати наскрізь.");
    }

    private static string Explanation(int step) => step switch
    {
        1 => "ЩО ЦЕ: дві базові будівлі Rock Raiders перекриваються зонами обслуговування й утворюють одну локальну мережу.",
        2 => "ЩО ЦЕ: T3-Trike змінює роль з «Ескорт» на «Розвідка» біля Сервісного центру. Перша купівля коштує 25 Ore + 10 Energy і триває 18 секунд.",
        3 => "ЩО ЦЕ: Resonance Core перетворює встановлені Crystals на накопичуваний Заряд. Він витрачає 50 Заряду на тимчасовий 18-секундний Сплеск навколо себе.",
        4 => "ЩО ЦЕ: три легкі Martian-юніти їдуть від Аеротрубного ангара до Поселення. У трубі вони тимчасово перебувають поза мапою, але не знищуються.",
        5 => "ЩО ЦЕ: Excavation Searcher відштовхує ворожу машину. Після першого поштовху ціль отримує 8 секунд захисту від повторного сильного відкидання.",
        6 => "ЩО ЦЕ: перед Hover Scout зараз стоїть справжня скельна стіна, яка блокує наземний шлях. Тест відкриє її та одразу накаже юніту проїхати наскрізь.",
        _ => string.Empty
    };

    private static string Instruction(int step) => step switch
    {
        1 => "РЕЗУЛЬТАТ УЖЕ ВИДНО: одна мережа має спільний доступ до ресурсів. Цей тест у тебе пройшов.",
        2 => "НАТИСНИ «ЗАПУСТИТИ»: стеж за відсотком. На 100% роль, огляд і швидкість зміняться, а модуль стане купленим саме цією машиною.",
        3 => "НАТИСНИ «ЗАПУСТИТИ»: через 0,75с стенд зупиниться в момент активації та покаже джерело й підсилений Razor Skimmer.",
        4 => "НАТИСНИ «ЗАПУСТИТИ»: статус покаже кожного пасажира, а після прибуття камера автоматично перейде до другої станції.",
        5 => "НАТИСНИ «ПОКАЗАТИ ПЕРШИЙ ПОШТОВХ»: побачиш рух на 2 клітинки. Потім ця сама кнопка покаже повторний рух лише на 0,5 клітинки.",
        6 => "СПОЧАТКУ ПОДИВИСЬ НА ВИДИМУ СТІНУ. Потім натисни «ВІДКРИТИ СКЕЛЬНУ СТІНУ»: вона зникне, а Hover Scout проїде крізь звільнене місце.",
        _ => string.Empty
    };

    private static int CountTransfers(SimulationWorld world)
    {
        int count = 0;
        foreach (EntityId id in world.Entities.Alive) if (world.Entities.TubeTransfer.Has(id)) count++;
        return count;
    }

    private static string TransferState(TubeTransferState state) => state switch
    {
        TubeTransferState.Approaching => "ЇДЕ ДО ВХОДУ",
        TubeTransferState.Queued => "ЧЕКАЄ В ЧЕРЗІ",
        TubeTransferState.Loading => "ЗАВАНТАЖУЄТЬСЯ",
        TubeTransferState.Travelling => "ЇДЕ ВСЕРЕДИНІ ТРУБИ",
        TubeTransferState.Unloading => "ВИВАНТАЖУЄТЬСЯ",
        TubeTransferState.ExitBlocked => "ЧЕКАЄ ВІЛЬНОГО ВИХОДУ",
        TubeTransferState.Returning => "ПОВЕРТАЄТЬСЯ ПІСЛЯ ОБРИВУ",
        TubeTransferState.ArrivalRecovery => "ПРИБУВ, ВІДНОВЛЮЄТЬСЯ",
        _ => state.ToString()
    };

    private static Button Button(string text)
    {
        Button button = new() { Text = text, CustomMinimumSize = new Vector2(0, 34) }; button.AddThemeFontSizeOverride("font_size", 13); return button;
    }

    private static Label Label(string text, int size, Color color)
    {
        Label label = new() { Text = text }; label.AddThemeFontSizeOverride("font_size", size); label.AddThemeColorOverride("font_color", color); return label;
    }

    private static string Charge(int millicharge) => $"{millicharge / 1000}.{(millicharge % 1000) / 100}";
}
