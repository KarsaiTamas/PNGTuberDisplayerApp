extends Node

var dbSavedCharacters: SQLite
var dbCharacter: SQLite 
var dbScene: SQLite 
var lastUsedScene=""
var sceneDataTable="scene_data"
#id
var characterID="character_id"
var outfitID="outfit_id"
var posX="pos_x"
var posY="pos_y"
var scale="scale"
var mirrored="mirrored"
var savedScenesTable="scenes"
#id
#name
#save_location 
#characters
var savedCharactersTable="characters" 
#id
#name
var fileName="save_location"
var cCharacterType="character_type" 
#outfits
var outfitsTable="outfits"
#id
#name
var isSimple="is_simple"
var outfitLocation="outfit_location"

var outfitAnimationsTable="outfit_animations"
var oOutfitID="outfit_id"
var oSpriteFile="sprite_file"
var oAnimLength="anim_length"
var oAnimCount="anim_count"
var oAnimType="anim_type"
var oAnimExtraInfo="extra_anim_info"

var outfitAnimsTable="character_animations"#needs anim id at the end
var lastUsedCharacter="character_"#needs character id at the end
var lastUsedSceneTable="last_used_scene"

var isSceneOpened=false
var isCharacterOpened=false

func _enter_tree():
	dbCharacter=SQLite.new()
	dbSavedCharacters=SQLite.new()
	dbScene=SQLite.new()
	#ChangeSaveFile(saveFile)
	dbSavedCharacters.path="res://saved_characters.db"
	dbSavedCharacters.open_db()
	Setup()
	#CreateTableSaveFiles()
#	InsertIntoSaveFile(saveFile)
#	ChangeSaveFileUsage(saveFile,true);
	#db.drop_table(buildingTableName) 
	#InsertIntoBuilding(0,1,1,1)
	#DeleteBuilding(1,1,1)
	#SelectBuildingByID("1")
	#UpdateBuilding("1",2,2,2)
	#SelectBuilding(1,1,1) 
func _ready() -> void:
	if(lastUsedScene!=-1):
		ChangeScene(lastUsedScene)
func ChangeScene(newScene):
	if(isSceneOpened):
		dbScene.close_db()
		isSceneOpened=false
	lastUsedScene=newScene
	dbScene.path="res://saves/"+lastUsedScene+".db"  
	dbScene.open_db()
	isSceneOpened=true
	
func ChangeUsedCharacter(newCharacter):
	if(isCharacterOpened):
		dbCharacter.close_db()
		isCharacterOpened=false
	lastUsedCharacter=newCharacter
	dbCharacter.path="res://saves/"+lastUsedCharacter+".db"  
	dbCharacter.open_db()
	isCharacterOpened=true
	
	
func CloseUsedCharacter():
	if(isCharacterOpened):
		dbCharacter.close_db()
		isCharacterOpened=false
	
func CloseUsedScene():
	if(isSceneOpened):
		dbScene.close_db()
		isSceneOpened=false
	
func Setup():
	CreateTableCharacters()
	CreateTableLastUsedScene()
	CreateTableScenes()
	lastUsedScene= SelectLastUsedScene()[0]["id"] 
func CreateTableCharacters(): 
	var table={
		"id":{"data_type":"int","primary_key":true,"not_null":true},
		"name":{"data_type":"VARCHAR(50)"},
		fileName:{"data_type":"TEXT"},
		cCharacterType:{"data_type":"int(4)"}
	}
	#dbSavedCharacters.drop_table(savedCharactersTable)
	dbSavedCharacters.create_table(savedCharactersTable,table)
	
func CreateTableScenes(): 
	var table={
		"id":{"data_type":"int","primary_key":true,"not_null":true},
		"name":{"data_type":"VARCHAR(50)"},
		fileName:{"data_type":"TEXT"},
	}
	#dbSavedCharacters.drop_table(savedCharactersTable)
	dbSavedCharacters.create_table(savedScenesTable,table)

	
func CreateTableOutfits(): 
	var table={
		"id":{"data_type":"int","primary_key":true,"not_null":true},
		"name":{"data_type":"VARCHAR(50)"},
		isSimple:{"data_type":"int(2)"},
		outfitLocation:{"data_type":"TEXT"}, 
	}
	#dbSavedCharacters.drop_table(savedCharactersTable)
	dbCharacter.create_table(outfitsTable,table)
	
	
func CreateTableOutfitsAnimations(): 
	var table={
		"id":{"data_type":"int","primary_key":true,"not_null":true},
		oOutfitID:{"data_type":"int","foreign_key":true,"not_null":true},
		"name":{"data_type":"VARCHAR(50)"},
		oAnimLength:{"data_type":"REAL"},
		oAnimCount:{"data_type":"int(8)"},
		oSpriteFile:{"data_type":"TEXT"}, 
		oAnimType:{"data_type":"VARCHAR(20)"}, 
		oAnimExtraInfo:{"data_type":"VARCHAR(50)"}, 
	}
	#dbSavedCharacters.drop_table(savedCharactersTable)
	dbCharacter.create_table(outfitAnimationsTable,table)
	
func CreateTableSceneData(): 
	var table={
		"id":{"data_type":"int","primary_key":true,"not_null":true},
		characterID:{"data_type":"int","foreign_key":true,"not_null":true},
		outfitID:{"data_type":"int","foreign_key":true,"not_null":true},
		posX:{"data_type":"REAL"},
		posY:{"data_type":"REAL"},
		scale:{"data_type":"REAL"},
		mirrored:{"data_type":"int(2)"},
	}
	#dbScene.drop_table(savedCharactersTable)
	dbScene.create_table(sceneDataTable,table)


func CreateTableLastUsedScene(): 
	var table={
		"id":{"data_type":"int"}
	} 
	#dbSavedCharacters.drop_table(lastUsedSceneTable)
	dbSavedCharacters.create_table(lastUsedSceneTable,table)
	var r=RunSavedCharactersSQLCommand("SELECT COUNT(*) as c FROM "+lastUsedSceneTable)
	if(r[0]['c']==0):
		RunSavedCharactersSQLCommand("INSERT INTO "+lastUsedSceneTable+" (id) VALUES(-1)")
		lastUsedScene=-1

func CreateCharacter(id:String,cName:String,type:String,saveLoc:String):
	InsertIntoCharacters(id,cName,type,saveLoc)
	ChangeUsedCharacter(saveLoc)
	CreateTableOutfits()
	CreateTableOutfitsAnimations()
	
	

func CreateScene(id:String,sName:String,saveLoc:String):
	InsertIntoScenes(id,sName,saveLoc)
	ChangeScene(saveLoc) 
	CreateTableSceneData()
	
func SelectLastUsedScene():
	return  RunSavedCharactersSQLCommand("SELECT id FROM "+lastUsedSceneTable)

func InsertIntoCharacters(id:String,cname:String,type:String,saveLoc:String):
	var data={
		"id":id,
		"name":cname,
		cCharacterType:type,
		fileName:saveLoc,
	}
	dbSavedCharacters.insert_row(savedCharactersTable,data)
	
func InsertIntoSceneData(id:String,cid:String,oid:String):
	var data={
		"id":id,
		characterID:cid,
		outfitID:oid,
		posX:0,
		posY:0,
		scale:1,
		mirrored:0,
	}
	dbScene.insert_row(sceneDataTable,data)
	
	
func InsertIntoScenes(id:String,sName:String,saveLoc:String):
	var data={
		"id":id,
		"name":sName,
		fileName:saveLoc,
	}
	dbSavedCharacters.insert_row(savedScenesTable,data)
	
func UpdateSceneData(id:String,oid:String,pX:float,pY:float,sc:float,mir:int):
	var data={
	outfitID:oid,
	posX:pX,
	posY:pY,
	scale:sc,
	mirrored:mir
	}
	var whereStateMent="id="+id
	dbScene.update_rows(sceneDataTable,whereStateMent,data)
	
		
func UpdateCharacter(id:String,cname:String,type:String):
	var data={
	"name":cname,
	cCharacterType:type
	}
	var whereStateMent="id="+id
	dbSavedCharacters.update_rows(savedCharactersTable,whereStateMent,data)
	
	
func UpdateLastUsedCScene(id:String):
	var data={
		"id":id,
	}
	var whereStateMent="id="+id
	dbSavedCharacters.update_rows(savedCharactersTable,whereStateMent,data)
	

func DeleteCharacter(id:String):
	var characterFileLocation= SelectCharacter(id)[0][fileName]
	
	dbSavedCharacters.delete_rows(savedCharactersTable,"id="+id)
	return characterFileLocation
	
	
func DeleteScene(id:String):
	var characterFileLocation= SelectScene(id)[0][fileName]
	dbSavedCharacters.delete_rows(savedScenesTable,"id="+id)
	return characterFileLocation
	
func SelectCharacter(id:String):
	return (dbSavedCharacters.select_rows(savedCharactersTable,"id="+id,["*"]))
	
func SelectCharacterInScene(id:String):
	return (dbScene.select_rows(sceneDataTable,"id="+id,["*"]))

func SelectScene(id:String):
	return (dbSavedCharacters.select_rows(savedScenesTable,"id="+id,["*"]))

 
#func InsertIntoSaveFile(saveFileName:String):
#	if(IsSaveExists(saveFileName)): return false;
#	var data={
#		sfName:saveFileName,
#		sfOpen:0,
#	}
#	dbSaveFiles.insert_row(savedFarmsTable,data)
#	return true;
	
#func ChangeSaveFileUsage(saveFileName:String,isInUse:bool):
#	dbSaveFiles.query("Update "+savedFarmsTable+
#		" SET "+sfOpen+"="+isInUse+
#		" WHERE "+sfName+"='"+saveFileName+"'");
#	print("Updated");
	
func RunSavedCharactersSQLCommand(commandText:String):
	dbSavedCharacters.query(commandText);
	return dbSavedCharacters.query_result;
func RunCharacterSQLCommand(commandText:String):
	dbCharacter.query(commandText);
	print(commandText)
	return dbCharacter.query_result;
	
func RunSceneSQLCommand(commandText:String):
	print(dbScene.path)
	dbScene.query(commandText);
	return dbScene.query_result;
	
	
#func IsSaveExists(saveFileName:String):
#	var whereStatement=sfName+"='"+saveFileName+"'"
#	dbSaveFiles.query("SELECT COUNT("+sfName+") AS C from "+savedFarmsTable+" WHERE "+whereStatement)
#	return dbSaveFiles.query_result[0]["C"]
	  
