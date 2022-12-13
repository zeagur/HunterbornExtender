Scriptname _DS_HB_mgef_AdvancedTaxonomy extends ActiveMagicEffect


_DS_HB_MAIN Property dsMain Auto
_DS_HB_Animals Property dsAnimals Auto
_DS_HB_Monsters Property dsMonsters Auto

Message property ConfirmChangeTaxon Auto
Message property ConfirmAddDeathItem Auto


String[] property animalNames auto
String[] property monsterNames auto
int[] property animalMapping auto
int[] property monsterMapping auto
bool property initialized auto


int[] animalEntries 
int[] monsterEntries


Event OnEffectStart(Actor akTarget, Actor akCaster)
	If !akTarget.IsDead()
		Return
	EndIf
	
	; Already classified by Hunterborn. Prompt the user to reclassify.
	if akTarget.GetItemCount(dsAnimals.kwDeathItemToken) > 0
		int choice = ConfirmChangeTaxon.Show()
		if choice != 1 
			return
		endIf
	endIf
	
	if !initialized
		initialized = true
		InitializeArrays()
	endIf
	
	UIListMenu menu = UIExtensions.GetMenu("UIListMenu") as UIListMenu
	int ANIMALS_MENU = menu.AddEntryItem("Animals")
	int MONSTERS_MENU = menu.AddEntryItem("Monsters")
	int CANCEL = menu.AddEntryItem("Cancel")

	int index = 0
	while index < animalNames.Length
		animalEntries[index] = menu.AddEntryItem(animalNames[index], ANIMALS_MENU)
		index += 1
	endWhile
	
	index = 0
	while index < monsterNames.Length
		monsterEntries[index] = menu.AddEntryItem(monsterNames[index], MONSTERS_MENU)
		index += 1
	endWhile
	
	menu.OpenMenu()
	int result = menu.GetResultInt()
	
	if result == CANCEL
		return
	endIf
	
	index = animalEntries.Find(result)
	if index >= 0
		TaxonomyAnimal(akTarget, animalMapping[index])
		return
	endIf
	
	index = monsterEntries.Find(result)
	if index >= 0
		TaxonomyMonster(akTarget, monsterMapping[index])
	endIf
	
EndEvent


Function TaxonomyAnimal(Actor akTarget, int index)
	If index >= 0
		;DebugMode("Taxonomized ANIMAL = " + index)
		akTarget.AddItem(dsAnimals.flDeathItemTokens.GetAt(index))
		dsMain.ActivateFreshCarcass(akTarget, index)

		ActorBase base = akTarget.GetLeveledActorBase()
		if base
			LeveledItem deathItem = PO3_SKSEFunctions.GetDeathItem(base)
			if deathItem == None 
				int result = ConfirmAddDeathItem.Show()
				if result == 1
					PO3_SKSEFunctions.SetDeathItem(base, dsAnimals.DeathItemLI[index])
				endIf
			endIf
		endIf
	EndIf
EndFunction


Function TaxonomyMonster(Actor akTarget, int index)
	If index >= 0
		;DebugMode("Taxonomized MONSTER = " + index)
		dsMonsters.InitTaxonomized(akTarget, index)
		dsMain.ActivateMonsterCarcass(akTarget)

		ActorBase base = akTarget.GetLeveledActorBase()
		if base
			LeveledItem deathItem = PO3_SKSEFunctions.GetDeathItem(base)
			if deathItem == None 
				int result = ConfirmAddDeathItem.Show()
				if result == 1
					PO3_SKSEFunctions.SetDeathItem(base, dsMonsters.DeathItemLI[index])
				endIf
			endIf
		endIf
	EndIf
EndFunction


int[] Function Bubblesort(String[] list)
	int[] mapping = Utility.CreateIntArray(list.Length)
	int count = mapping.length
	while count
		count -= 1
		mapping[count] = count
	endWhile
	
	int outer = list.length
	while outer > 1
		outer -= 1
		
		int inner = outer 
		while inner
			inner -= 1
			if (list[inner] > list[outer]) 
				Swap(list, mapping, inner, outer)
			endIf
		endWhile
	endWhile
	
	return mapping
EndFunction


Function Swap(String[] list, int[] mapping, int a, int b)
	String temp1 = list[a]
	list[a] = list[b]
	list[b] = temp1
	
	int temp2 = mapping[a]
	mapping[a] = mapping[b]
	mapping[b] = temp2
EndFunction


Function InitializeArrays()
	Debug.Notification("There will be a short delay while the taxonomy records are initialized.")
	
	animalEntries = Utility.CreateIntArray(dsAnimals.AnimalIndex.Length)
	monsterEntries = Utility.CreateIntArray(dsMonsters.MonsterIndex.Length)
	
	animalNames = Utility.CreateStringArray(dsAnimals.AnimalIndex.Length)
	monsterNames = Utility.CreateStringArray(dsMonsters.MonsterIndex.Length)
	
	int index
	
	index = animalNames.Length
	while index
		index -= 1
		animalNames[index] = dsAnimals.AnimalIndex[index]
	endWhile
	
	index = monsterNames.Length
	while index
		index -= 1
		monsterNames[index] = dsMonsters.MonsterIndex[index]
	endWhile
	
	animalMapping = Bubblesort(animalNames)
	monsterMapping = Bubblesort(monsterNames)
EndFunction
