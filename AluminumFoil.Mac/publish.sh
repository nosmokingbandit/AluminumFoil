rids=("win7-x64" "linux-x64" "osx-x64" "exit")

select opt in "${rids[@]}"
do
    case $opt in
        "win7-x64")
			RID=$opt
            break
            ;;
        "linux-x64")
			RID=$opt
            break
            ;;
        "osx-x64")
			RID=$opt
            break
            ;;
        "exit")
            exit
            ;;
        *) echo "invalid option $REPLY";;
    esac
	
done

echo "Publishing for ${RID}"

dotnet publish -c Release -f netcoreapp2.0 -r ${RID} --self-contained