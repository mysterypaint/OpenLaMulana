# Execute all the scripts in this folder

for x in ./*
do
        if [ "$x" != "$0" ]
        then
            ./$x
        fi
done
