#include "merdog_runner.h"
#include "../interpreter/include/environment.hpp"
#include <codecvt>
using namespace MerdogRT;
using namespace Platform;

Platform::String^ stringToPlatformString(std::string inputString) {
	std::wstring_convert<std::codecvt_utf8<wchar_t>> converter;
	std::wstring intermediateForm = converter.from_bytes(inputString);
	Platform::String^ retVal = ref new Platform::String(intermediateForm.c_str());

	return retVal;
}

std::string convert_from_string(String^ tmp)
{
	std::wstring tmp2(tmp->Begin());
	return std::string(tmp2.begin(), tmp2.end());
}
void MerdogRunner::run()
{
	Mer::run_interpreter(convert_from_string(code_content));
}

String^ MerdogRT::MerdogRunner::get_output_buf()
{
	return stringToPlatformString(Mer::output_buff);
}

Platform::String^ MerdogRT::MerdogRunner::get_error_content()
{
	return stringToPlatformString(err_msg);
}
