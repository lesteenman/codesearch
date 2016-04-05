#!/bin/bash

show_help() {
	echo "Usage: cs regex [-(c|C)] [-w]"
	echo ""
	echo "case insensitive searches will be used if the search string contained only lowercase and numericals."
	echo ""
	echo "    -h: Show this help."
	echo "    -C: Force case sensitive searching."
	echo "    -c: Force case insensitive searching."
	echo "    -w: Exact word match."
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
	DEFAULT_CAPS=true
	DEBUG=false
	C=""
	W=""
	# Default to case insensitive search if no caps were used

	while getopts "hcCwdv" opt; do
		case "$opt" in
			h|\?)
				show_help
				exit 0
				;;
			C)
				C=""
				DEFAULT_CAPS=false
				;;
			c)
				C="i"
				DEFAULT_CAPS=false
				;;
			w)
				W="w"
				;;
			d|v)
				DEBUG=true
				;;
		esac
	done
	shift $(expr $OPTIND - 1 )

	# Default to case insensitive if only lower case was used
	if [[ $DEFAULT_CAPS == true && "$@" == $@{,,} ]]; then
		C="i"
	fi

	exclude_string=""
	for d in ${exclude_directories[@]}; do
		exclude_string="$exclude_string --exclude-dir=$d"
	done
	for f in ${exclude_files[@]}; do
		exclude_string="$exclude_string --exclude=$f"
	done

	SEARCH="$@"
	if [[ $DEBUG == true ]]; then
		echo "Default caps: $DEFAULT_CAPS"
		echo "Arguments: -TInr$C$W"
		echo "Exclude files: ${exclude_files[*]}"
		echo "Exclude dirs: ${exclude_directories[*]}"
		echo "Full exclude string: $exclude_string"
		echo "Search term: $SEARCH"
	fi

	out=$(grep -TInr$C$W --color=always \
		$exclude_string \
		"$SEARCH")
	# set -x
	# out=$(grep -TInr$C$W --color=always --exclude=${exclude_files} --exclude-dir=${exclude_directories} "$SEARCH")
	# for ex in "${exclude_files[@]}"; do
	# 	out=$(echo $out | grep -v "$ex")
	# done
	# for ex in "${exclude_directories[@]}"; do
	# 	out=$(echo $out | grep -v "$ex/")
	# done
	# set +x

	if [ "$out" != "" ]; then
		echo "$out" | less -RX
	else
		echo "Nothing matched '$1'"
	fi
fi
