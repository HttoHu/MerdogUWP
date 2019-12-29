#pragma once
#include <string>
extern std::string err_msg;
extern int err_line;
namespace MerdogRT
{

    public ref class MerdogRunner sealed
    {
    public:
		MerdogRunner(Platform::String^ str) :code_content(str) {}
		void run();
		Platform::String^ get_output_buf();
		int line_no() { return err_line; }
		bool have_error() { return !err_msg.empty(); }
		Platform::String^ get_error_content();
	private:
		Platform::String^ code_content;
    };
}
