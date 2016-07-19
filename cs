#!/bin/bash

show_help() {
	echo "Usage: cs regex [-(c|C)] [-w] [-n]"
	echo ""
	echo "case insensitive searches will be used if the search string contained only lowercase and numericals."
	echo ""
	echo "    -h: Show this help."
	echo "    -C: Force case sensitive searching."
	echo "    -c: Force case insensitive searching."
	echo "    -w: Exact word match."
	echo "    -s: Search for exact string."
	echo "    -f [pattern]: Only search files matching pattern."
	echo "    -n: Number of matches."
	echo "    -N: Number of matches per file."
	echo "    -d: Debug mode."
}

exclude_directories={}
exclude_files={}
if [ -f "$HOME/.cs_config" ]; then
	# Example config:
	# if [[ $PWD == "$HOME/codesearch"* ]]; then
	# 	exclude_directories=(.git)
	# 	exclude_files=(.gitconfig tags)
	# fi
	source $HOME/.cs_config
fi

if [ -z "$1" ]; then
	show_help
else
	OPTIND=1
	DEBUG=false
	C="-S"
	W=""
	Q=""
	G=""
	FILEPATTERN=""
	# Default to case insensitive search if no caps were used

	while getopts "hcCwdvnNf:s" opt; do
		case "$opt" in
			h|\?)
				show_help
				exit 0
				;;
			C)
				C="-s"
				;;
			c)
				C="-i"
				;;
			w)
				W="-w"
				;;
			d|v)
				DEBUG=true
				;;
			n)
				COUNT=true
				;;
			N)
				COUNTPERFILE=true
				;;
			f)
				if [ -z "$FILEPATTERN" ]; then
					FILEPATTERN="${OPTARG}"
				else
					FILEPATTERN="$FILEPATTERN|${OPTARG}"
				fi
				;;
			s)
				Q="-Q"
		esac
	done
	shift $(expr $OPTIND - 1 )

	exclude_string="--ignore=*.socket"
	for d in ${exclude_directories[@]}; do
		exclude_string="$exclude_string --ignore-dir=$d"
	done
	for f in ${exclude_files[@]}; do
		exclude_string="$exclude_string --ignore=$f"
	done

	if [ -n "$FILEPATTERN" ]; then
		G="-G $FILEPATTERN"
	fi

	SEARCH="$@"
	PREARGUMENTS="$C $Q $W $exclude_string"
	POSTARGUMENTS="$G"
	if [[ $DEBUG == true ]]; then
		echo "Pre-Arguments: $PREARGUMENTS"
		echo "Post-Arguments: $POSTARGUMENTS"
		echo "Exclude files: ${exclude_files[*]}"
		echo "Exclude dirs: ${exclude_directories[*]}"
		echo "Full exclude string: $exclude_string"
		echo "Search term: $SEARCH"
	fi

	if [[ $COUNT == true ]]; then
		out=$(ag --stats --color $PREARGUMENTS "$SEARCH" $POSTARGUMENTS | ag '[0-9] (files contained )?matches')
		echo "$out"
	elif [[ $COUNTPERFILE == true ]]; then
		out=$(ag -c --color $PREARGUMENTS "$SEARCH" $POSTARGUMENTS | awk -F : ' {count = gsub(/\x1b/, "\x1b"); if (count == 0) count += 40; else count += 50; printf "%-"count"s %s\n", $1, $2}')
		echo "$out"
	else
		out=$(ag $PREARGUMENTS --color --group --color-path='36' --color-match='91' "$SEARCH" $POSTARGUMENTS)

		if [ -n "$out" ]; then
			clear
			echo "$out" | sed -e 's/\([0-9]*:\)[ 	]*/\1 /' | less -RX
		else
			echo "Nothing matched '$SEARCH'"
		fi
	fi
fi
